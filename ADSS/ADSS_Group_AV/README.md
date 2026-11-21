# Superli — Store Management System

A command-line management system for **Superli**, a supermarket chain. It combines two modules that share one SQLite database:

- **Inventory:** categories, products, stock locations, defective items, discounts, reports and low-stock alerts
- **Human Resources:** employees, branches, roles, availability, and shift scheduling

Built as a team project (4 students) for the **Analysis and Design of Software Systems (ADSS)** course at Ben-Gurion University. The design work (requirements, UML, use cases, contracts, sequence diagrams) is in [`docs/`](docs).

![Java](https://img.shields.io/badge/Java-17-orange) ![Maven](https://img.shields.io/badge/build-Maven-blue) ![SQLite](https://img.shields.io/badge/DB-SQLite-lightgrey) ![Tests](https://img.shields.io/badge/tests-71%20passing-brightgreen)

---

## Features

### 📦 Inventory module
- **Category tree:** root categories and sub-categories of any depth
- **Item types (products):** manufacturer, cost and selling price, store location, shelf and warehouse quantities, minimum quantity
- **Items:** add single or bulk items, move items between **shelf** and **warehouse**, track expiration dates, mark and unmark items as **damaged**, remove all defective items at once
- **Discounts:** on specific item types or on whole categories
- **Reports:** inventory by category, defective items, and a **purchasing report** of what needs to be reordered
- **Alerts:** raised automatically when an item type drops below its minimum quantity
- **Supplier integration** through a supplier mock

### 👥 HR module
- **Employees:** register and log in, update personal details, bank account and employment terms
- **Branches:** every employee and every shift belong to one store branch
- **Roles and availability:** employees submit availability, and managers define the roles a shift requires
- **Shift scheduling:** create shifts, assign employees, and check that a shift is valid (all required roles are filled, from the same branch)
- **Transport support:** a `Driver` employee type with a license type; queries for available drivers and for whether a **Stock Keeper** is on shift when a delivery arrives

---

## Architecture

The project is a multi-module Maven build:

```
dev/
├── InventoryModel/   inventory module   (package adss.inventory)
├── hr-module/        HR module          (package superli)
└── system-app/       entry point that combines both modules into one app
```

Each module is built in layers:

```
Presentation (CLI)  →  Service  →  Domain  →  DAO / Repository  →  SQLite
```

- **Domain:** the business rules (e.g. `Item`, `ItemType`, `Category`, `Discount`, `Employee`, `Shift`, `StoreBranch`)
- **DTOs** carry data between layers, and **DAOs** hold all SQL
- The design follows **GRASP** principles. For example, `Warehouse` was promoted to its own class as an *Information Expert*. The design rationale is in [`docs/Readme.txt`](docs/Readme.txt).

**Database:** 15 tables, including `categories`, `item_types`, `items`, `discounts`, `alerts`, `employees`, `employee_roles`, `availability`, `branches`, `shifts`, `shift_assignments` and `drivers`. They are created automatically on first run.

---

## Getting started

### Requirements
- Java 17 or newer
- Maven 3.8 or newer (only needed to build from source)

### Run the ready-made release
```bash
cd release
java -jar adss2025_v02.jar
```

### Build from source and run the tests
```bash
cd dev
mvn package                                # compiles all modules and runs all 71 tests
java -jar system-app/target/adss2025_v02.jar
```

### Using the app
```
==========================================
    SUPERLI STORE MANAGEMENT SYSTEM
==========================================
1. Inventory Management Module
2. Human Resources (HR) Module
3. Exit System
```
- **Inventory:** choose to start with an empty store or load previous data from the database.
- **HR → Manager:** the demo access code is `9999`.

---

## Testing
**71 JUnit 5 tests** in total:
- **20** in the inventory module
- **51** in the HR module, including domain tests, DAO tests against a test database, controller tests, and integration tests that use a transport mock

```bash
cd dev && mvn test
```

---

## Documentation
| Folder | Contents |
|---|---|
| [`docs/requirements`](docs/requirements) | Functional and non-functional requirements |
| [`docs/use-cases`](docs/use-cases) | Use case diagram and detailed use cases |
| [`docs/class-diagram`](docs/class-diagram) | UML class diagram |
| [`docs/sequence-diagrams`](docs/sequence-diagrams) | Sequence diagrams (add item, remove item, assign employee to shift) |
| [`docs/contracts`](docs/contracts) | Operation contracts |

---

## Tech stack
Java 17 · Maven (multi-module) · SQLite (`sqlite-jdbc`) · JUnit 5 · draw.io UML
