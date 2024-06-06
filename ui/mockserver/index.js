const express = require("express");
const app = express();

// port number for the server
const port = 8012;

const fs = require("fs");
const bodyParser = require("body-parser");

app.use(bodyParser.json()); // to support JSON-encoded bodies
app.use(
  bodyParser.urlencoded({
    // to support URL-encoded bodies
    extended: true,
  })
);

// read secrets from file
const secrets = JSON.parse(fs.readFileSync("secrets.json", "utf8"));

// read description from file
const description = fs.readFileSync("description.html", "utf8");

// enable CORS
app.use((_, res, next) => {
  res.header("Access-Control-Allow-Origin", "*");
  res.header("Access-Control-Allow-Headers", "*");
  next();
});

// define a route handler for the default home page
app.get("/", (_, res) => {
  res.send(description);
});

// define a route handler for the health endpoint
app.get("/health", (_, res) => {
  res.send({ status: "UP" });
});

// define a route handler for the secret endpoint
app.post("/api/Secret", async (req, res) => {
  console.log("Saving secret:\n ", req.body);
  const secret = req.body;

  // simulate a delay if needed
  await sleep(100);

  // validate secret
  if (secret.PublicPin === undefined || secret.Content === undefined) {
    console.log("Invalid secret");
    return res.status(400).send("Invalid secret");
  }

  // generate a random id
  const id = Math.random().toString(36).substring(7);

  // save secret
  secrets.push({
    id: id,
    secret: secret.Content,
    pin: secret.PublicPin,
  });

  // return id
  res.send(id);
});

// define a route handler for the secret endpoint with id parameter and pin query parameter
// e.g. /api/Secret/1?pin=1234
app.get("/api/Secret/:id", async (req, res) => {
  console.log(
    "Requesting secret with id:",
    req.params.id + " and pin:",
    req.query.pin
  );

  // simulate a delay if needed
  await sleep(100);

  const pin = req.query.pin;
  const secretId = req.params.id;

  // find secret by id
  const secret = secrets.filter((s) => s.id === secretId)[0];

  // if secret not found, return 404
  if (secret === undefined) {
    console.log("Secret not found");
    return res.status(404).send("Secret not found");
  }

  console.log("secret:", secret);

  // if pin is incorrect, return 400
  if (secret.pin !== pin) {
    console.log("Invalid pin");
    return res.status(400).send("Invalid pin");
  }

  // return secret
  return res.send(JSON.stringify(secret));
});

app.listen(port, () => {
  console.log(`Mockserver is listening on port ${port}`);
});

function sleep(ms) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}