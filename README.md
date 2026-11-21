# Software Engineering B.Sc. — Course Projects

Projects and assignments from my B.Sc. in Software Engineering at **Ben-Gurion University of the Negev**.
They cover systems programming, operating systems, compilers, programming languages, and object-oriented design, in Java, C/C++, C#, OCaml, TypeScript, Python and assembly.

Most projects were built in teams of 2–4 students.

---

## ⭐ Highlighted projects

| Project | What it is | Tech |
|---|---|---|
| [Superli store management system](ADSS/ADSS_Group_AV) | Multi-module store system with **inventory** and **HR / shift scheduling** modules on a shared SQLite database. Layered architecture (presentation → service → domain → DAO). 71 unit tests. | Java, Maven, SQLite, JUnit 5 |
| [Scheme → x86-64 compiler](Compilation/final_project) | Compiler for a subset of Scheme: parser combinators, reader, tag-parser, semantic analysis (lexical addressing, tail-call annotation) and x86-64 assembly code generation. | OCaml, NASM |
| [xv6 kernel extensions](operating_systems) | Three assignments extending the xv6-RISC-V teaching OS: new system calls, coroutines, a kernel PRNG, synchronization primitives, and framebuffer display output over virtio-gpu. | C, RISC-V, QEMU |
| [Kanban board](intro_to_Software_engeenering/kanban-2025-2025-32-main) | Kanban task management application: users, boards, columns with limits, tasks and board membership. Layered backend with persistence and a WPF desktop frontend (MVVM). 27 unit tests. | C#, .NET, WPF, SQLite |
| [Microservices framework](SPL/assignment_2) | A message-bus based microservices framework, used to run a multi-threaded sensor fusion (SLAM) simulation of cameras, LiDAR and GPS/IMU that maps landmarks. | Java (concurrency), Gson, JUnit 5 |
| [STOMP client & server](SPL/assignment_3) | A STOMP messaging server (thread-per-client and Reactor modes) and a C++ client that reports events over STOMP. | Java, C++ |

---

## Courses

### ADSS — Analysis and Design of Software Systems
[`ADSS/ADSS_Group_AV`](ADSS/ADSS_Group_AV): the Superli store management system described above. Includes UML class and sequence diagrams, use cases, and operation contracts under `docs/`.

```bash
cd ADSS/ADSS_Group_AV/dev
mvn package                                    # builds and runs all tests
java -jar system-app/target/adss2025_v02.jar   # or run release/adss2025_v02.jar
```

### Compilation
| Folder | Contents |
|---|---|
| [`HW1`](Compilation/HW1) | Parser combinators and parsing exercises |
| [`HW23`](Compilation/HW23) | Scheme reader, tag-parser and semantic analysis |
| [`final_project`](Compilation/final_project) | The full Scheme → x86-64 compiler (`make` + NASM, Linux) |
| [`parsers`](Compilation/parsers) | Practice with parser combinators |

### Operating Systems (xv6-RISC-V)
| Folder | Topics |
|---|---|
| [`assignment_1`](operating_systems/assignment_1) | First system calls: `memsize`, coroutines (`co_yield`) |
| [`assignment_2`](operating_systems/assignment_2) | Kernel LCG random number generator, process groups, custom locks, a multi-process "relay race" |
| [`assignment_3`](operating_systems/assignment_3) | Mapping a framebuffer into user space over virtio-gpu; Game of Life display |

Run with `make qemu` on Linux, using the RISC-V toolchain and QEMU (a devcontainer is included).

### SPL — Systems Programming
| Folder | Contents | Tech |
|---|---|---|
| [`assignment_1`](SPL/assignment_1) | Settlement and facility construction simulation, using rule-of-five resource management | C++ |
| [`assignment_2`](SPL/assignment_2) | Microservices framework and sensor fusion simulation | Java |
| [`assignment_3`](SPL/assignment_3) | STOMP server and client | Java, C++ |
| [`assignment_4`](SPL/assignment_4) | Store database with a persistence layer (DTO/DAO/Repository) | Python, SQLite |

### LAB — Systems Programming Lab
C and assembly labs on Linux: a shell with pipes and job control (`Lab_2`, `Lab_c`), assembly programming (`Lab_3`, `Lab_d`), binary patching and hex editing (`Lab_4`), an ELF parser (`Lab_e`), an ELF loader (`Lab_5`), and a virus-signature scanner (`Lab_b`).

### PPL — Principles of Programming Languages
Interpreters and type systems written in TypeScript:
- **`assignment_1`:** functional programming in TypeScript
- **`assignment_2`:** extending the L3 interpreter (L31, L32)
- **`assignment_3`:** the L5 type checker and **type inference**, plus logic programming in Racket
- **`assignment_4`:** lazy lists and CPS in Racket, promises and generators in TypeScript, and logic programming in Prolog

### OOP — Object-Oriented Programming
[`assignment_3`](OOP/assignment_3): a turn-based dungeon crawler for the terminal, with several player classes, enemies, bosses and traps. It is built with the Visitor, Observer and Factory design patterns and has JUnit tests.

```bash
java -jar OOP/assignment_3/hw3.jar <levels-folder>
```

### Introduction to Software Engineering
[`kanban-2025-2025-32-main`](intro_to_Software_engeenering/kanban-2025-2025-32-main): the Kanban application described above (`dotnet build Kanban.sln`, `dotnet test`).

---

## Contact
**Hadi Alhmamdh**: Software Engineering student, Ben-Gurion University
GitHub: [@hadialhmamdh](https://github.com/hadialhmamdh)
