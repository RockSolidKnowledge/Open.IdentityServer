# Security Policy

Security is a fundamental consideration for Open.IdentityServer.

Open.IdentityServer is an OAuth 2.0 and OpenID Connect framework used to build authentication and authorisation solutions. We take reports of potential security vulnerabilities seriously and encourage responsible disclosure.

## Reporting a Security Vulnerability

If you believe you have discovered a security vulnerability in Open.IdentityServer, please report it privately to our security team.

**Email:** vulnerabilities@identityserver.com

Please **do not create a public GitHub issue, discussion, pull request, or otherwise publicly disclose the vulnerability** until we have had an opportunity to investigate and, where necessary, provide a fix or mitigation.

### What to Include

Please provide as much information as possible to help us reproduce and investigate the issue, including:

- The affected Open.IdentityServer version.
- A description of the vulnerability and its potential impact.
- Steps required to reproduce the issue.
- A proof of concept, where practical.
- Relevant configuration or environmental information.
- Any suggested mitigation or remediation, if known.
- Whether you believe the vulnerability has already been publicly disclosed or is being actively exploited.
- Your preferred contact details for follow-up.

Reports consisting solely of automated vulnerability scanner output may not provide enough information for us to confirm an issue. Where possible, please demonstrate how the reported behaviour could be exploited in the context of Open.IdentityServer.

## What Happens Next

When we receive a security report, we will:

1. **Acknowledge the report** and begin an initial assessment.
2. **Investigate and validate the issue**, including the affected versions, potential impact and severity.
3. **Develop a fix or mitigation** when a vulnerability is confirmed within Open.IdentityServer.
4. **Keep the reporter informed** of material progress.
5. **Coordinate disclosure** to give users an appropriate opportunity to update or apply mitigations.
6. **Publish a security advisory and CVE** for confirmed vulnerabilities where a security update or mitigation is required. The CVE will provide information about the affected versions and the appropriate remediation or mitigation.

Confirmed vulnerabilities that require a security update or mitigation will be assigned and disclosed through a CVE. We will coordinate publication of the vulnerability with the availability of the appropriate update or mitigation wherever practicable.

## Responsible Disclosure

We ask security researchers to work with us on coordinated disclosure.

Please provide us with a reasonable opportunity to investigate and remediate an issue before publishing technical details that could enable exploitation.

We appreciate the contribution made by security researchers who help improve Open.IdentityServer. Where appropriate, and with the researcher's permission, we may publicly acknowledge their contribution.

## Scope

This policy applies to vulnerabilities in the Open.IdentityServer framework itself.

Open.IdentityServer is a framework rather than a complete identity solution. The security of a deployed solution also depends on factors including:

- Application-specific configuration.
- Custom application code.
- Authentication mechanisms.
- User interfaces.
- Hosting and infrastructure.
- Databases and other data stores.
- Third-party dependencies and integrations.
- Operational and deployment practices.

A vulnerability caused solely by application-specific configuration, custom code, infrastructure, or another component may therefore fall outside the scope of an Open.IdentityServer vulnerability.

If you are unsure whether an issue originates in Open.IdentityServer or your implementation, we would still encourage you to contact us privately so that we can help determine its scope.

## Security Testing

Open.IdentityServer undergoes regular security testing, including independent penetration testing of the framework

In addition to ongoing security review during development, Open.IdentityServer is subject to regular independent penetration testing. The purpose of this testing is to identify potential vulnerabilities in the framework and provide additional assurance around its security.

Security testing of Open.IdentityServer should not, however, be considered a substitute for security testing of applications built using it.

**Open.IdentityServer is a framework, not a complete identity solution**. The security of a deployed solution depends on how the framework is configured and integrated, together with the security of the surrounding application, infrastructure, authentication mechanisms, user interfaces, dependencies and operational environment.

We therefore strongly recommend that organisations perform appropriate security testing, including penetration testing, of their complete deployed solution, particularly before production deployment and following significant changes.

A penetration test of Open.IdentityServer itself cannot provide assurance that a particular implementation or deployment of Open.IdentityServer is secure.