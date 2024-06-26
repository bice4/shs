import { Button } from "primereact/button";
import { useParams } from "react-router";
import { InputTextarea } from "primereact/inputtextarea";
import { InputOtp } from "primereact/inputotp";
import React, { useState, useRef } from "react";
import { Toast } from "primereact/toast";
import axios from "axios";
import { Link } from "react-router-dom";
import { BlockUI } from "primereact/blockui";
import { ProgressSpinner } from "primereact/progressspinner";

// SecretViewer component to view a secret by providing a password
// and show the secret content if the password is correct
export default function SecretViewer() {
  const { id } = useParams();
  const toast = useRef(null);

  const [content, setContent] = useState("");
  const [pinCode, setPinCode] = useState("");
  const [pinCodeVisible, setPinCodeVisible] = useState(true);
  const [isUiBlocked, setIsUiBlocked] = useState(false);

  // open secret
  function open() {
    // validate password. if empty, show error
    if (pinCode === null || pinCode.length === 0) {
      showError("Password can not be empty.");
      return;
    }

    // add cors header
    axios.defaults.headers.post["Access-Control-Allow-Origin"] = "*";

    setIsUiBlocked(true);

    // send get request to get secret
    axios
      .get(process.env.REACT_APP_API_URL + "/Secret/" + id + "?pin=" + pinCode)
      .then((response) => {
        if (response.status !== 200) {
          // if response status is 404, show error secret is not found
          if (response.status === 404) {
            showError("Secret is not found", "Error");
            return;
          }

          // if response status is not 200, show error failed to get a secret
          showError("Test", "Failed to get a secret");
          return;
        }

        // set secret content
        // clear password
        // hide password input
        setContent(response.data.secret);
        setPinCode("");
        setPinCodeVisible(false);
        setIsUiBlocked(false);
      })
      .catch((e) => {
        setIsUiBlocked(false);

        // if response status is 404, show error secret is not found
        let errorMessage = "Something went wrong";

        if (e?.response?.data !== null) {
          errorMessage = e.response.data;
        }
        showError(errorMessage, "Failed to get a secret");
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

  return (
    <div className="flex flex-column">
      <Toast ref={toast} />

      <div className="flex md:flex-auto align-items-center justify-content-center font-bold m-2">
        <h2 className="text-4xl">Secret store demo</h2>
      </div>
      {pinCodeVisible && (
        <BlockUI blocked={isUiBlocked}>
          <div>
            <div className="flex align-items-center justify-content-center h-8rem font-bold m-2">
              <div className="flex align-items-stretch flex-wrap">
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
              </div>
            </div>
            <div className="flex align-items-center justify-content-center font-bold m-2">
              <Button label="Open" onClick={open} text />
            </div>
          </div>
        </BlockUI>
      )}
      {isUiBlocked && <ProgressSpinner />}
      {content !== null && content.length !== 0 && (
        <div className="flex md:flex-auto align-items-center flex-column justify-content-center font-bold m-2">
          <div className="flex md:flex-auto flex-column gap-2">
            <label htmlFor="username">Secret content</label>
            <InputTextarea
              id="username"
              aria-describedby="username-help"
              style={{ resize: "none", width: "100%" }}
              value={content}
              rows={10}
              cols={100}
            />
          </div>
        </div>
      )}
      <div className="flex align-items-center justify-content-center h-4rem font-bold m-2">
        <Link
          className="p-button p-component p-button-text"
          id="home-link"
          to="/"
        >
          Home
        </Link>
      </div>
    </div>
  );
}
