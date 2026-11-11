# EI Accounting Performance Assessment -- Next-Phase Instructions

## 1. Database Access and Assessment

-   The database connection string is available in `appsettings.json`.
-   You are authorized to connect to the database and inspect its schema
    and database objects.
-   The database is currently empty except for lookup/reference data.
-   Review every database object, including:
    -   Tables
    -   Views
    -   Stored procedures
    -   Functions
    -   Triggers
    -   Indexes
    -   Constraints
    -   Other relevant database objects
-   Identify relationships, dependencies, potential performance issues,
    missing indexes, inefficient queries, and architectural concerns.
-   Check whether Query Store is enabled and assess the availability of
    execution statistics and database logs.
-   Clearly distinguish verified findings from potential issues that
    cannot be confirmed without production-like data or workload
    evidence.
-   Do not expose connection strings, credentials, or other secrets in
    reports or generated artifacts.

## 2. Performance Testing with 5 Million Invoices

Establish a realistic performance-testing environment.

### 2.1 Data Model and Test-Data Preparation

-   Identify the required tables, relationships, mandatory fields, and
    business rules for creating a client and its invoices.
-   Create a dedicated test client in the database.
-   Prepare a data-generation approach to insert 5 million invoices for
    that client, including all required related records.
-   Ensure generated data is realistic and respects existing business
    rules, constraints, and relationships.
-   Do not bypass business logic or introduce inconsistent financial
    data merely to generate volume.
-   Before inserting data, calculate the estimated storage requirements
    in MB and GB.
-   Include estimates for:
    -   Invoice records
    -   Related records
    -   Indexes
    -   Transaction log growth
    -   Reasonable additional database overhead
-   Identify database growth, transaction log, and performance
    considerations associated with bulk insertion.
-   Present the proposed data-generation and loading strategy, together
    with the storage estimate, for review before executing it.

### 2.2 Performance Assessment After Data Generation

Once the test data is available, execute the application's reports
against the test client and identify:

-   The slowest reports.
-   Reports that fail or time out.
-   Execution time and resource consumption, where measurable.
-   SQL queries and stored procedures responsible for bottlenecks.
-   Scalability issues as data volume increases.

Also identify slow or failing application pages where measurable.

Do not claim tenant-specific production findings based solely on the
test client. Clearly label all results as local test-environment
findings.

## 3. Command Timeout Policy

Finite command timeouts are approved, based on evidence and
recommendations.

Before changing timeout values:

-   Review existing command-timeout configurations and execution
    patterns.
-   Recommend appropriate timeout values based on operation type.
-   Consider reports, bulk operations, integrations, and normal
    transactional operations separately.
-   Identify queries that currently run for unusually long periods.
-   Do not use timeout changes to conceal underlying performance
    problems.
-   Document the behavioral impact of timeout changes.
-   Ensure appropriate error handling and logging are in place.

## 4. Report File Delivery

Changing the report file-delivery mechanism is approved, provided that
generated report contents remain identical.

Preserve all of the following:

-   Report data
-   Calculations and financial values
-   Formatting and layout
-   File formats
-   Existing report-generation business rules

Changes must improve delivery performance, reliability, or memory
consumption without changing report contents.

## 5. General Instructions and Change Controls

-   Continue following the original EI Accounting performance
    optimization and refactoring instruction document.
-   Preserve all existing business functionality, accounting
    calculations, and multi-tenant isolation.
-   **Do not delete files or database objects.**
-   Do not make unrelated architectural changes.
-   Keep the assessment evidence-based and document measurable results.
-   Separate approved changes from recommendations that still require
    approval.
-   Maintain a clear record of database changes and test-data
    generation.

## 6. Local Development and Performance-Testing Environment

All assessment and performance-testing activities are currently being
performed on the local development machine.

-   Use the existing local application, SQL Server instance, database,
    and configuration.
-   Do not assume that a production or UAT environment is available.
-   Do not access or modify any remote production or UAT environment.

Before executing resource-intensive operations:

-   Check available disk space, SQL Server memory, CPU resources, and
    current database size.
-   Estimate storage and transaction-log requirements for generating 5
    million invoices.
-   Present the estimates and proposed execution strategy before
    proceeding.
-   Avoid operations that could affect other local databases or
    applications.

Clearly distinguish local benchmark results from expected production
performance. Do not extrapolate local results into production guarantees
without appropriate qualification and supporting evidence.

## 7. Required Starting Point and Execution Sequence

**Start by reviewing the database structure and calculating the storage
requirements for the proposed 5-million-invoice test. Present your
findings and execution plan before loading any test data.**

Follow this sequence:

1.  Review the existing application and the original EI Accounting
    performance optimization and refactoring instructions.
2.  Inspect the local environment and confirm available disk, memory,
    CPU, and database capacity.
3.  Inventory and assess all relevant database objects, relationships,
    and dependencies.
4.  Identify the data model and business rules required to create the
    test client, invoices, and related records.
5.  Calculate estimated storage requirements, including data, indexes,
    transaction log, and overhead.
6.  Prepare the data-generation and loading strategy, including
    safeguards and a rollback/cleanup approach that does not delete
    existing files or database objects.
7.  Present the database assessment, storage estimates, risks, and
    proposed execution plan for review.
8.  **Do not begin the 5-million-invoice data load until the proposed
    approach and estimates have been presented and approved.**
9.  After approval and data generation, execute the report and
    application performance tests, capture evidence, and document
    results.
10. Propose code, query, index, timeout, or report-delivery changes
    separately, clearly identifying which are already approved and which
    require further approval.

## 8. Reporting and Evidence Requirements

For each finding, record where applicable:

-   Finding ID and severity
-   Affected application page, report, database object, or process
-   Evidence and reproduction steps
-   Measured execution time or resource usage
-   Root cause, if verified
-   Whether the finding is verified or only a potential issue
-   Recommended remediation
-   Expected impact and any trade-offs
-   Approval status
-   Validation or regression-test results

Do not present assumptions as measured facts. If a metric cannot be
collected in the local environment, state that limitation explicitly.
