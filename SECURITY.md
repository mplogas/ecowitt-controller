# Security Policy

## Supported Versions

The following versions of Ecowitt Controller currently receive security updates:

| Version | Supported          |
| ------- | ------------------ |
| latest (`main`), 2.0.x releases | ✅ Yes |
| <2.0 releases  | ❌ No  |

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

### Preferred: GitHub Private Vulnerability Reporting

Use GitHub's built-in private reporting to disclose vulnerabilities directly and confidentially to the maintainer:

1. Go to the [Security tab](https://github.com/mplogas/ecowitt-controller/security) of this repository.
2. Click **"Report a vulnerability"**.
3. Fill in as much detail as possible (see below).

This creates a private advisory draft that only you and the maintainer can see, until a fix is published.

---

## What to Include in Your Report

To help assess and address the issue quickly, please include:

- A description of the vulnerability and its potential impact
- Steps to reproduce or a proof-of-concept
- Affected versions or configurations
- Any suggested mitigations (if known)

---

## What to Expect

- **Acknowledgement** within 48–72 hours of your report
- **Status updates** as the issue is investigated and a fix is developed
- **Credit** in the security advisory (if you wish) when the vulnerability is disclosed
- Public disclosure will happen **after a fix is available**, coordinated with you

---

## Scope

This project is a self-hosted home automation bridge between Ecowitt weather stations and MQTT/Home Assistant. Security issues of particular interest include:

- Malicious or forged payloads submitted to the HTTP endpoint (`POST /data/report`), including injection attacks or crafted input that could cause unintended behaviour
- MQTT credential handling or exposure
- Dependency vulnerabilities with a credible attack vector in this context

Issues related to misconfiguration by the user (e.g., exposing the service publicly without a reverse proxy) are generally considered out of scope, but feedback is still welcome.
