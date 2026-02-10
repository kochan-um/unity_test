## ADDED Requirements
### Requirement: Unity WebGL build automation
The system SHALL build Unity WebGL artifacts in GitHub Actions on every push to the default branch (`main` or `master`) and on manual workflow dispatch.

#### Scenario: Push to default branch triggers build
- **WHEN** a commit is pushed to `main` or `master`
- **THEN** the workflow runs a Unity WebGL build job using the configured Unity version and project path

#### Scenario: Manual trigger executes build
- **WHEN** an operator triggers `workflow_dispatch`
- **THEN** the same WebGL build steps run without requiring an additional commit

### Requirement: Deterministic WebGL output for deployment
The system SHALL locate and expose a single deterministic WebGL output directory from the Unity build artifacts for downstream deployment.

#### Scenario: Build output is discovered from generated index.html
- **WHEN** Unity build completes under the configured build root
- **THEN** the workflow identifies the directory containing `index.html` and exports it as deployment input

#### Scenario: Missing output fails the pipeline
- **WHEN** no WebGL output directory containing `index.html` is found
- **THEN** the workflow fails with an explicit error and does not start deployment

### Requirement: Vercel production deployment
The system SHALL deploy the discovered WebGL output directory to Vercel production as part of the same workflow execution.

#### Scenario: Successful deployment with valid token
- **WHEN** `VERCEL_TOKEN` is configured and build output exists
- **THEN** the workflow runs `vercel deploy --prod --yes` against the output directory

#### Scenario: Missing token blocks deployment
- **WHEN** `VERCEL_TOKEN` is not configured
- **THEN** the workflow exits with a clear error before attempting deployment

### Requirement: Secrets validation before build and deploy
The system SHALL validate required Unity and Vercel secrets before dependent steps execute.

#### Scenario: Unity license prerequisites are checked
- **WHEN** both `UNITY_LICENSE` and `UNITY_SERIAL` are missing
- **THEN** the workflow fails early and instructs maintainers to set Unity-related secrets

#### Scenario: Optional Vercel organization scope
- **WHEN** `VERCEL_ORG_ID` is present
- **THEN** deployment includes the org scope argument

### Requirement: Correct content headers for compressed WebGL assets
The system SHALL configure static headers so compressed WebGL assets are served with matching `Content-Encoding` and `Content-Type` values.

#### Scenario: Brotli and gzip assets are served with correct metadata
- **WHEN** `.wasm.br`, `.js.br`, `.data.br`, `.wasm.gz`, `.js.gz`, or `.data.gz` files are requested
- **THEN** Vercel responses include the appropriate `Content-Encoding` and `Content-Type` headers for each extension