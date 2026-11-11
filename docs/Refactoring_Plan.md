# EI Accounting – Performance Optimization and Refactoring

## 1. Role

Act as a Senior Software Architect and Performance Engineering Expert with extensive experience in:

- .NET application architecture and performance optimization.
- SQL Server database optimization.
- Multi-tenant application architecture.
- Accounting and financial application development.
- Production performance troubleshooting.
- Application modernization and incremental refactoring.
- Financial transaction processing and data integrity.

You are working directly with the existing **EI Accounting** application in the current project workspace.

## 2. Project Context

EI Accounting is an established accounting application currently used by numerous customers in a multi-tenant environment.

The application is experiencing significant performance issues:

- Application response times are slow.
- Reports take too long to generate.
- Some reports fail with errors or timeouts.
- Performance may vary across tenants.
- The existing .NET application and SQL Server database require comprehensive assessment and optimization.

The objective is to improve application performance, scalability, reliability, and maintainability while preserving all existing business functionality and ensuring that changes do not negatively impact other tenants.

## 3. Primary Objective

Perform a comprehensive assessment of the existing EI Accounting application, including:

- Existing .NET application.
- SQL Server database.
- Database schema.
- Stored procedures.
- Queries and indexes.
- Reporting implementation.
- Application architecture.
- Multi-tenant implementation.
- Existing business logic and workflows.
- External integrations.

Develop an evidence-based, phased optimization and refactoring plan.

**The primary objective is to identify and address the actual root causes of performance issues without changing existing business logic, accounting calculations, workflows, or functional behavior.**

Do not immediately start modifying the application or database.

First, understand the existing implementation, identify the root causes, establish performance baselines, and prepare a detailed optimization strategy.

---

# 4. Business Logic and Existing Functionality Preservation

EI Accounting is an established accounting application with numerous existing business rules, financial calculations, validations, workflows, integrations, and customer-specific configurations.

Performance optimization and code refactoring must not change, remove, bypass, simplify, or unintentionally alter any existing business logic or functional behavior.

**Strictly follow these requirements:**

### 4.1 Understand Existing Business Logic

Before modifying any existing functionality:

1. Thoroughly understand the associated business rules.
2. Identify dependencies and execution flow.
3. Understand inputs, outputs, and side effects.
4. Identify downstream impacts.
5. Understand customer-specific configurations.
6. Identify integrations and dependent processes.
7. Determine how the functionality is currently used.

Do not modify existing functionality based on assumptions.

### 4.2 Preserve Existing Accounting and Business Rules

Preserve all existing business rules, including but not limited to:

- Invoice creation and processing rules.
- Payment application, unapplication, and reapplication logic.
- Accounts Receivable (AR) workflows.
- Accounts Payable (AP) workflows.
- Commission calculations and payment distributions.
- Premium, fee, tax, and other financial calculations.
- Journal entry creation and posting logic.
- Invoice due date calculations and payment terms.
- Payment status and invoice status transitions.
- Overpayment and overapplication rules.
- Bill creation, cancellation, retry, and failure-handling logic.
- Accounting period validations.
- Customer-specific business rules and configurations.
- Existing authorization and access-control validations.
- External accounting integration workflows.

These are examples of critical functionality, not an exhaustive list.

Identify and preserve all additional business rules discovered in the existing implementation.

### 4.3 Financial Calculation Preservation

Do not change any existing:

- Calculation formulas.
- Financial rounding rules.
- Decimal precision.
- Currency handling.
- Date calculations.
- Payment allocation logic.
- Commission calculations.
- Premium calculations.
- Tax calculations.
- Invoice totals.
- Journal entry calculations.
- Accounting validations.

Do not introduce floating-point conversions or other changes that could affect financial precision.

Do not change calculation order if it can affect the final financial result.

Any financial calculation optimization must produce the same result as the existing implementation for equivalent inputs and business scenarios.

### 4.4 Preserve Existing Validations and Conditions

Do not remove, bypass, simplify, or alter existing:

- Validation conditions.
- Business-rule checks.
- SQL joins.
- SQL filters.
- Authorization checks.
- Tenant filters.
- Transaction boundaries.
- Processing sequences.
- Error-handling conditions.
- Status transition validations.

Do not assume that existing conditions or joins are unnecessary simply because they appear to negatively affect performance.

Some implementation details may exist to support specific accounting scenarios or customer requirements.

### 4.5 Preserve Existing Processing Sequences

Before optimizing any stored procedure, API, service, or business logic, identify its:

- Inputs.
- Outputs.
- Side effects.
- Dependencies.
- Transaction boundaries.
- Processing sequence.
- Error-handling behavior.
- Expected results.

Do not reorder processing steps if doing so could affect accounting results, transaction consistency, or downstream integrations.

Do not change transaction boundaries or isolation levels without understanding their business and data-integrity implications.

### 4.6 Preserve Existing Integrations

Preserve all existing integrations and their expected behavior, including:

- API requests.
- API response handling.
- Authentication.
- Authorization.
- Retry mechanisms.
- Error handling.
- Data mapping.
- Request sequencing.
- External accounting workflows.
- Integration-specific validations.

Do not change integration behavior as part of performance optimization unless the change is separately documented and explicitly approved.

### 4.7 Customer-Specific Configuration Preservation

EI Accounting is a multi-tenant application.

Do not modify, remove, or override:

- Tenant-specific configurations.
- Customer-specific business rules.
- Accounting preferences.
- Financial settings.
- Integration configurations.
- Existing customer-specific workflows.

Before proposing a change, determine whether the affected functionality is used differently by different tenants.

Ensure that optimizations do not introduce tenant-specific regressions.

### 4.8 Existing Behavior Must Remain the Reference

The existing implementation is the reference for business behavior.

Any proposed optimization must produce the same functional results as the existing implementation for equivalent inputs and business scenarios.

Do not introduce new business rules, modify existing workflows, or change customer-specific configurations without explicit approval.

If an existing business rule is unclear:

1. Document the uncertainty.
2. Identify the affected functionality.
3. Explain the potential impact.
4. Request clarification.
5. Do not make assumptions or change the behavior.

### 4.9 Mandatory Functional Equivalence Validation

Before implementing any optimization that affects business logic, financial calculations, data processing, or transaction behavior:

1. Identify the existing behavior.
2. Document the existing inputs and outputs.
3. Identify relevant business scenarios.
4. Establish expected results using the current implementation.
5. Prepare appropriate regression tests.
6. Compare the optimized implementation against the existing implementation.
7. Verify that financial calculations and accounting outcomes remain unchanged.
8. Verify that downstream processing and integrations remain unaffected.

Where appropriate, use the existing implementation as a reference for comparison.

For financial calculations, validate exact results, including precision and rounding behavior.

Do not consider a performance improvement successful if it introduces functional differences.

### 4.10 Mandatory Requirement

**The primary objective is to improve performance without changing how EI Accounting currently works from a business and accounting perspective.**

Treat existing business functionality as fixed and performance as the optimization target.

Any change that could affect business behavior must be:

- Separately documented.
- Supported by a clear technical explanation.
- Evaluated for business and accounting impact.
- Reviewed and explicitly approved before implementation.

Do not combine unapproved functional changes with performance optimization work.

---

# 5. Phase 1 – Existing EI Accounting Application Assessment

Start by analyzing the existing EI Accounting project and its database implementation.

Do not assume the architecture or technology stack.

Identify the actual implementation from the available source code, project files, database scripts, and configuration.

## 5.1 Application Architecture

Review:

- Overall application architecture.
- Existing .NET framework/version.
- Solution and project structure.
- Controllers.
- Services.
- Business logic.
- Data-access patterns.
- Database connection management.
- Connection pooling.
- Transaction management.
- Application-level caching.
- Exception handling.
- Existing logging.
- Background processing.
- Scheduled tasks.
- External integrations.

Understand how requests flow through the application and interact with the database.

## 5.2 SQL Server Database

Review:

- Database schema.
- Tables.
- Relationships.
- Data volumes.
- Stored procedures.
- Views.
- Functions.
- Indexes.
- Query execution patterns.
- Transaction handling.
- Database maintenance.
- Data growth.
- Tenant-specific data access patterns.

Identify database objects that are frequently accessed or potentially responsible for performance issues.

## 5.3 Reporting Implementation

Review:

- Report-generation architecture.
- Report controllers and services.
- SQL queries and stored procedures.
- Data retrieval patterns.
- Report execution times.
- Memory consumption.
- Report timeouts.
- Report errors.
- Concurrent report execution.
- Tenant-specific reporting workloads.

Understand the complete execution flow for important reports.

## 5.4 Multi-Tenant Architecture

Review:

- Tenant identification.
- Tenant-specific data access.
- Tenant filtering.
- Tenant isolation.
- Database access patterns.
- Tenant-specific configurations.
- Shared resources.
- Resource contention.
- Tenant-specific performance differences.

Determine whether one tenant's workload can negatively impact other tenants.

## 5.5 Business Logic Assessment

Identify the existing accounting workflows and business rules.

Understand the relationships between:

- Invoice processing.
- Payment processing.
- AR and AP.
- Commission processing.
- Financial calculations.
- Journal entries.
- Accounting periods.
- Integrations.
- Customer-specific configurations.

Document critical workflows and their dependencies.

Do not modify these workflows during the assessment.

---

# 6. Phase 2 – Performance Analysis and Root Cause Identification

Investigate the actual causes of performance issues across EI Accounting.

## 6.1 Application Performance

Analyze:

- Slow application pages and endpoints.
- Excessive database calls.
- N+1 query patterns.
- Inefficient data processing.
- Synchronous operations.
- Memory consumption.
- CPU utilization.
- Large response payloads.
- Inefficient object allocation.
- Application-level caching opportunities.
- External API latency.
- Connection pool exhaustion.

Identify measurable performance bottlenecks.

## 6.2 Database Performance

Analyze:

- Slow-running queries.
- Stored procedure execution times.
- Query execution plans.
- Missing or ineffective indexes.
- Table scans.
- Inefficient joins.
- Parameter sniffing.
- Blocking.
- Deadlocks.
- Long-running transactions.
- Database connection pressure.
- TempDB utilization.
- Large tables.
- Data growth.
- Tenant-specific query performance.

Determine whether the performance problems are caused by query execution, database resource constraints, application behavior, or a combination of factors.

## 6.3 Reporting Performance

Identify:

- Reports with consistently high execution times.
- Reports that fail or time out.
- Reports retrieving excessive data.
- Inefficient joins and aggregations.
- Large datasets processed in application memory.
- Concurrent report execution issues.
- Tenant-specific data volume differences.
- Opportunities for pagination.
- Opportunities for batching.
- Opportunities for pre-aggregation.
- Opportunities for background processing.

Establish measurable performance baselines.

Distinguish between:

1. Confirmed performance issues.
2. Potential issues requiring investigation.
3. Architectural limitations.
4. Improvements that can be implemented without major refactoring.

Prioritize issues based on actual evidence.

---

# 7. Phase 3 – Multi-Tenant Performance and Scalability

Evaluate EI Accounting specifically from a multi-tenant perspective.

Investigate:

- Whether tenant-specific queries are properly optimized.
- Whether tenant filters are consistently applied.
- Whether indexes support tenant-based access patterns.
- Whether large tenants negatively impact smaller tenants.
- Whether database connections and resources are shared efficiently.
- Whether tenant-specific caching is appropriate.
- Whether report execution requires concurrency limits.
- Whether tenant workloads need prioritization or isolation.

Recommend practical improvements to tenant isolation, workload management, and scalability.

Ensure that every optimization preserves existing tenant-specific business rules and data isolation.

Do not introduce changes that could expose one tenant's data to another tenant.

---

# 8. Phase 4 – SQL Server Database Optimization and Refactoring

Develop a structured database optimization strategy for EI Accounting.

Evaluate:

- Database schema.
- Stored procedures.
- Views.
- Functions.
- Query performance.
- Index design.
- Execution plans.
- Statistics maintenance.
- Transaction management.
- Blocking and deadlocks.
- Data retrieval patterns.
- Report queries.
- Database growth.
- Data archival and retention opportunities.

For every proposed database optimization, identify:

1. Existing implementation.
2. Performance issue.
3. Root cause.
4. Recommended change.
5. Expected performance improvement.
6. Potential risks.
7. Business logic impact.
8. Functional equivalence requirements.
9. Validation approach.
10. Rollback strategy.

Do not remove existing joins, conditions, filters, or validations merely to improve query performance.

Do not change stored procedure behavior without explicitly documenting and validating the impact.

Avoid unnecessary schema redesign or rewriting stored procedures without measurable justification.

---

# 9. Phase 5 – .NET Application Refactoring

Develop a practical refactoring strategy for the existing EI Accounting application.

Evaluate:

- Application architecture.
- Controllers.
- Services.
- Business logic.
- Data-access implementation.
- Database connection handling.
- Asynchronous operations.
- Memory consumption.
- Caching.
- Exception handling.
- Report-generation workflows.
- External integrations.
- Resource management.

Recommend improvements that align with the existing implementation.

Prioritize incremental refactoring over a complete rewrite unless the assessment demonstrates that a redesign is necessary.

For each proposed change:

- Identify the affected files.
- Explain the current implementation.
- Explain the performance issue.
- Describe the proposed optimization.
- Identify business logic dependencies.
- Explain how existing behavior will be preserved.
- Define testing requirements.
- Define rollback considerations.

Do not simplify or restructure business logic without understanding its purpose.

---

# 10. Phase 6 – Reporting Optimization

Develop a dedicated strategy for improving report performance in EI Accounting.

Address:

- Slow report queries.
- Report timeouts.
- Large datasets.
- Database-intensive calculations.
- Concurrent report execution.
- Memory-intensive processing.
- Long-running report requests.
- Tenant-specific reporting workloads.

Evaluate appropriate solutions such as:

- SQL query optimization.
- Stored procedure optimization.
- Pagination.
- Batch processing.
- Asynchronous report generation.
- Background processing.
- Caching.
- Pre-aggregation.

Explain when each approach is appropriate and its potential trade-offs.

Identify the reports that should be prioritized based on:

- Execution time.
- Frequency.
- Customer impact.
- Failure rate.
- Data volume.
- Business importance.

**Ensure that report optimizations do not change report calculations, financial totals, filtering, grouping, ordering, or existing output behavior.**

---

# 11. Phase 7 – Phased Implementation Plan

Prepare a structured implementation roadmap for EI Accounting.

For each phase, provide:

1. Objective.
2. Activities.
3. Application files that may require changes.
4. Database objects that may require changes.
5. Dependencies.
6. Expected outcome.
7. Risks.
8. Business logic impact.
9. Validation criteria.
10. Rollback considerations.

Prioritize improvements based on:

- Business impact.
- Performance improvement potential.
- Implementation complexity.
- Production risk.
- Tenant impact.

Ensure that changes can be implemented incrementally without disrupting existing customers.

Separate performance-only changes from changes that could affect functional behavior.

Do not combine unrelated refactoring activities into a single large implementation.

---

# 12. Phase 8 – Testing and Validation

Define a comprehensive testing strategy covering:

- Functional regression testing.
- Application performance testing.
- Database performance testing.
- Report-generation testing.
- Multi-tenant isolation testing.
- Concurrent user testing.
- Load testing.
- Stress testing.
- Memory and resource utilization.
- Production-like data volume testing.
- Accounting workflow validation.
- Financial calculation validation.
- Integration testing.

Establish measurable performance baselines and acceptance criteria.

## Functional Equivalence

For every optimization that affects existing functionality:

1. Identify the original behavior.
2. Define relevant test scenarios.
3. Capture expected results.
4. Execute the existing implementation where appropriate.
5. Execute the optimized implementation.
6. Compare outputs.
7. Verify accounting calculations.
8. Verify database side effects.
9. Verify status transitions.
10. Verify external integration behavior.

Ensure that all existing business rules remain intact.

For financial calculations, validate exact values, precision, and rounding behavior.

Do not consider a performance improvement successful if it introduces functional differences.

---

# 13. Phase 9 – Production Deployment and Monitoring

Recommend a safe production rollout strategy covering:

- Development and testing environments.
- Database migration management.
- Backward compatibility.
- Incremental deployment.
- Tenant-specific validation.
- Production monitoring.
- Error tracking.
- Performance metrics.
- Rollback procedures.
- Post-deployment validation.

Explain how to identify regressions and ensure performance remains stable as tenant count and data volumes increase.

Ensure that production deployment does not introduce unapproved business logic changes.

---

# 14. Phase 10 – Expected Deliverables

Provide the following deliverables specifically for EI Accounting:

1. Existing application architecture assessment.
2. Application and database performance assessment.
3. Root cause analysis.
4. Identified bottlenecks and severity.
5. Prioritized optimization recommendations.
6. SQL Server database refactoring plan.
7. .NET application refactoring plan.
8. Reporting optimization plan.
9. Multi-tenant scalability recommendations.
10. Business logic dependency assessment.
11. Functional equivalence validation strategy.
12. Phased implementation roadmap.
13. Testing and validation strategy.
14. Production deployment and rollback strategy.
15. Performance monitoring recommendations.

---

# 15. Important Instructions for Claude

- Work directly with the existing EI Accounting application.
- Do not assume the root cause of performance issues.
- Do not recommend a complete rewrite without sufficient justification.
- Do not start modifying files or database objects during the initial assessment.
- Analyze the actual implementation before proposing solutions.
- Distinguish confirmed issues from potential issues.
- Prioritize measurable performance improvements.
- Consider the impact of every proposed change on existing tenants.
- Avoid unnecessary third-party dependencies.
- Preserve existing business functionality.
- Treat existing accounting behavior as fixed.
- Do not simplify existing financial calculations.
- Do not remove existing business-rule validations.
- Do not change processing sequences without approval.
- Do not modify customer-specific configurations.
- Do not change integration behavior without approval.
- Do not delete any existing files or database objects.
- Do not rename, remove, or replace existing functionality without explicit approval.
- Maintain a clear record of identified issues, proposed changes, and validation results.
- Do not make assumptions about the architecture, database schema, or reporting implementation.
- If required information is unavailable, identify what is missing instead of guessing.
- Ask for clarification whenever existing business behavior is unclear.
- Do not introduce functional changes as part of performance optimization.

**Performance optimization must not compromise accounting accuracy, data integrity, tenant isolation, or existing customer workflows.**

---

# 16. How to Start

Begin by reviewing the existing EI Accounting application in the current project workspace.

Analyze the available:

- Solution and project structure.
- .NET application code.
- Controllers.
- Services.
- Business logic.
- Data-access implementation.
- SQL Server database scripts.
- Stored procedures.
- Views.
- Report-generation implementation.
- Configuration.
- External integrations.

## Initial Assessment

First, provide:

1. Your understanding of the existing EI Accounting architecture.
2. The current application and database structure.
3. The critical accounting workflows and their dependencies.
4. The initial performance investigation plan.
5. The methodology for identifying and prioritizing root causes.
6. The areas requiring deeper analysis.
7. The proposed phased optimization and refactoring approach.
8. The risks associated with performance optimization.
9. The strategy for preserving existing business functionality.
10. The proposed functional equivalence testing approach.

Identify which areas can be optimized without affecting business behavior and which areas require additional investigation or approval.

## Approval Requirement

**Do not begin implementation or modify any files or database objects until the initial assessment is complete and I have reviewed and approved the proposed approach.**

During the initial assessment:

- Do not edit files.
- Do not execute database modifications.
- Do not delete files.
- Do not rename files.
- Do not remove existing functionality.
- Do not change business logic.
- Do not introduce new business rules.

After completing the assessment, present the findings and wait for approval.

Once approved, implement the optimization plan incrementally, validating performance improvements and functional equivalence after each phase.

## Final Objective

Establish a reliable, evidence-based performance optimization and refactoring strategy for the existing EI Accounting application.

Improve application performance, database efficiency, reporting speed, scalability, and maintainability while maintaining production stability and protecting all existing tenants.

**Existing EI Accounting business functionality, accounting calculations, financial accuracy, workflows, validations, integrations, and customer-specific configurations must remain unchanged unless I explicitly approve a separate functional change.**