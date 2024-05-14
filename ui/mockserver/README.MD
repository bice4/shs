## Introduction

This repository contains a mock server for the secret store. It is designed to simulate the behavior of the actual secret store for testing and development purposes.

## Getting Started

To get started with the mock server, follow these steps:

1. Install the required dependencies: `npm install`
2. Start the mock server: `npm start`

## Usage

Once the mock server is running, you can interact with it using the following endpoints:

- `GET` `/api/secrets/:id?pin=` - Retrieve a specific secret by ID with Pin
- `POST` `/api/secrets` - Create a new secret

## Configuration

The mock server has `secrets.json` with predifined secrets. You can use it for dev porpuses.
With `POST` endpoint you can add a new secret. It will be added in memory cache and **not available after restart**.
