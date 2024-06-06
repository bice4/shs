import { InputTextarea } from "primereact/inputtextarea";
import React, { useState, useRef } from "react";
import { Dropdown } from "primereact/dropdown";
import { InputText } from "primereact/inputtext";
import { Button } from "primereact/button";
import { Toast } from "primereact/toast";
import axios from "axios";
import { Link } from "react-router-dom";
import { InputOtp } from "primereact/inputotp";
import { BlockUI } from "primereact/blockui";
import { ProgressSpinner } from "primereact/progressspinner";

// SecretCreator component to create a secret and show the secret link
export default function SecretCreator() {
  const toast = useRef(null);

  const [secretLink, setSecretLink] = useState("");
  const [content, setContent] = useState("");
  const [pinCode, setPinCode] = useState("");
  const [isUiBlocked, setIsUiBlocked] = useState(false);

  const [isCreated, setIsCreated] = useState(false);

  const dateValues = [
    { name: "Minutes", code: "M" },
    { name: "Hours", code: "H" },
    { name: "Days", code: "D" },
  ];
  const [selectedMeasurement, setSelectedMeasurement] = useState({
    name: "Minutes",
    code: "M",
  });

  const [measurementValue, setMeasurementValue] = useState(10);

  // create secret
  function create() {
    console.log("API URL: ", process.env.REACT_APP_API_URL);

    // validate content. if empty, show error
    if (content === null || content.length === 0) {
      showError("Secret content can not be empty.");
      return;
    }

    // validate password. if empty, show error
    if (pinCode === null || pinCode.length === 0) {
      showError("Password can not be empty.");
      return;
    }

    // validate expire in. if less than or equal to 0, show error
    if (measurementValue <= 0) {
      showError("Expire in should be greater than zero.");
      return;
    }

    // validate expire in. if greater than 99, show error
    if (selectedMeasurement.code === "D" && measurementValue > 99) {
      showError("Expiration date is too big.");
      return;
    }

    // calculate expire date
    const currentDate = new Date();
    const expireDate = new Date();

    // calculate expire date based on selected measurement
    switch (selectedMeasurement.code) {
      case "M":
        expireDate.setMinutes(currentDate.getMinutes() + measurementValue);
        break;
      case "H":
        expireDate.setHours(currentDate.getHours() + measurementValue);
        break;
      case "D":
        expireDate.setDate(currentDate.getDate() + measurementValue);
        break;
      default:
        showError("Invalid expiration date");
        return;
    }

    // send request
    sendRequest(content, pinCode, expireDate);
  }

  function sendRequest(content, pinCode, expireDate) {
    setIsUiBlocked(true);
    // compose secret object
    const secret = {
      Content: content,
      PublicPin: pinCode,
      ExpirationDate: expireDate,
    };
    // add cors header
    axios.defaults.headers.post["Access-Control-Allow-Origin"] = "*";

    // send request to create secret
    axios
      .post(process.env.REACT_APP_API_URL + "/Secret", secret)
      .then((response) => {
        // if response status is not 200, show error
        if (response.status !== 200) {
          showError("Failed to create secret", "Error");
          return;
        }

        // set secret link
        const hostName = window.location.origin;
        setSecretLink(hostName + "/Secret/" + response.data);
        showSuccess("Secret created");

        // set is created to true, so we can hide the form
        setIsCreated(true);
        navigator.clipboard.writeText(hostName + "/Secret/" + response.data);

        setContent("");
        setPinCode("");
        setMeasurementValue(10);
        setSelectedMeasurement({ name: "Minutes", code: "M" });
        setIsUiBlocked(false);
      })
      .catch((_) => {
        setIsUiBlocked(false);
        showError("Failed to create secret", "Error");
      });
  }

  // show error message in toast
  const showError = (message, summary = "Validation error") => {
    toast.current.show({
      severity: "error",
      summary: summary,
      detail: message,
    });
  };

  // show success message in toast
  const showSuccess = (message) => {
    toast.current.show({
      severity: "success",
      summary: "Success",
      detail: message,
    });
  };

  return (
    <div className="flex md:flex-auto flex-column font-bold">
      <Toast ref={toast} />

      <div className="flex md:flex-auto align-items-center justify-content-center font-bold m-2">
        <h2 className="text-4xl">Secret store demo</h2>
      </div>

      {!isCreated && (
        <BlockUI blocked={isUiBlocked}>
          <div className="flex md:flex-auto align-items-center flex-column justify-content-center m-2">
            <div className="flex md:flex-auto flex-column gap-2">
              <label htmlFor="username">Secret content</label>
              <InputTextarea
                id="username"
                aria-describedby="username-help"
                style={{ resize: "none", width: "100%" }}
                value={content}
                rows={10}
                cols={100}
                onChange={(e) => setContent(e.target.value)}
              />
            </div>

            <div className="flex align-items-stretch flex-wrap m-3">
              <div className="flex align-items-center justify-content-center font-bold border-round m-2">
                <div className="flex flex-column gap-2">
                  <label htmlFor="pwd1">Pin code (only numbers)</label>
                  <InputOtp
                    value={pinCode}
                    onChange={(e) => setPinCode(e.value)}
                    mask
                    integerOnly
                    length={6}
                  />
                </div>
              </div>
              <div className="flex align-self-start align-items-center justify-content-center font-bold border-round m-2">
                <div className="flex flex-column gap-2">
                  <label htmlFor="pwd1">Expire in</label>
                  <Dropdown
                    value={selectedMeasurement}
                    onChange={(e) => setSelectedMeasurement(e.value)}
                    options={dateValues}
                    optionLabel="name"
                    className="w-full md:w-10rem"
                  />
                </div>
              </div>
              <div className="flex align-items-center justify-content-center font-bold border-round m-2">
                <div className="flex flex-column gap-2">
                  <label htmlFor="pwd1">{selectedMeasurement.name}</label>
                  <InputText
                    value={measurementValue}
                    onChange={(e) => setMeasurementValue(e.target.value)}
                    keyfilter="int"
                    maxLength={99}
                    min={1}
                    max={10}
                  />
                </div>
              </div>
            </div>
            <div className="flex align-items-center justify-content-center font-bold m-2">
              <Button label="Create" onClick={create} text />
            </div>
          </div>
        </BlockUI>
      )}
      {isUiBlocked && <ProgressSpinner />}
      {secretLink !== null && secretLink.length > 0 && (
        <div>
          <div className="flex align-items-center justify-content-center font-bold m-2">
            <span>
              Your secret was created. Link to secret saved in the clipboard.
            </span>
          </div>
          <div className="flex align-items-center justify-content-center font-bold m-2">
            <Link
              className="p-button p-component p-button-text"
              id="home-link"
              to={secretLink}
            >
              Open link
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}
