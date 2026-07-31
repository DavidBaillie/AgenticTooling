---
name: dotnet-nunit
description: 'Get best practices for NUnit unit testing and integration testing, including data-driven tests'
---

# NUnit Best Practices

Your goal is to help me write effective unit tests with NUnit, covering both standard and data-driven testing approaches.

## Project Setup

- Reference Microsoft.NET.Test.Sdk, NUnit, and NUnit3TestAdapter packages
- Create test classes that match the classes being tested (e.g., `CalculatorTests` for `Calculator`)
- Use .NET SDK test commands: `dotnet test` for running tests

## Test Structure

- Apply `[TestFixture]` attribute to test classes
- Use `[Test]` attribute for test methods
- Follow the Arrange-Act-Assert (AAA) pattern
- Name tests using the pattern `MethodName_Scenario_ExpectedBehavior`
- Use `[SetUp]` and `[TearDown]` for per-test setup and teardown
- Use `[SetUpFixture]` for assembly-level setup and teardown

## Standard Tests

- Keep tests focused on a single behavior
- Avoid testing multiple behaviors in one test method
- Use clear assertions that express intent
- Include only the assertions needed to verify the test case
- Make tests independent and idempotent (can run in any order)
- Avoid test interdependencies
- All tests should be setup for parallelization via the `[Parallelizable(ParallelScope.All)]` attribute

## Data-Driven Tests

- Use `[TestCase]` for inline test data
- Use `[TestCaseSource]` for programmatically generated test data
- Use `[Values]` for simple parameter combinations
- Use `[ValueSource]` for property or method-based data sources
- Use `[Random]` for random numeric test values
- Use `[Range]` for sequential numeric test values
- Use `[Combinatorial]` or `[Pairwise]` for combining multiple parameters

## Assertions

- Assert using the `Shouldly` package
  - Nuget Package: `https://www.nuget.org/packages/shouldly/`
  - Project Repository: `https://github.com/shouldly/shouldly`
  - Documentation: `https://docs.shouldly.org`
- Use descriptive messages in assertions for clarity on failure

## Mocking and Isolation

- Mock dependencies to isolate units under test
  - When you require a mock, check to see if there is a sandbox implementation for the interface 
  - Sandbox implementations are concrete classes with the `Sandbox` postfix
- Use interfaces to facilitate mocking
- Consider using a DI container for complex test setups

## Test Organization

- Group tests by feature or component
- Use categories with `[Category("CategoryName")]`
- Do not use use `[Order]` to control test execution order. All tests must run in parallel.
- Use `[Author("DeveloperName")]` to indicate ownership
- Use `[Description]` to provide additional test information
- Consider `[Explicit]` for tests that shouldn't run automatically
- Use `[Ignore("Reason")]` to temporarily skip tests
