# SmartLogger - Build Observability with Less Complixity :)

> ***Before you begin, I want to clarify something:***
> I didn’t build SmartLogger to replace existing logging libraries.
> I built it to understand how they actually work under the hood.

## Why I Built This

While working with logging frameworks, I realized something:

> We use logging every day… but rarely think about how it behaves when things go wrong.

Questions started coming up:

* What happens when thousands of logs are written at the same time?
* How do logs stay consistent across multiple threads?
* How can configuration change without restarting the app?
* What if the logging system itself fails?

Instead of just reading about these, I decided to **build one from scratch**.

That’s how SmartLogger started.

## What This Project Is About

SmartLogger is a **learning-focused project** where I explored how real-world systems are designed.

Instead of chasing features, I focused on:

* Keeping logging **safe and predictable**
* Making behavior **clear and understandable**
* Designing for **real-world scenarios like concurrency and failures**

## What I Explored

Through this project, I tried to understand how systems handle:

* Multiple threads writing logs at the same time
* Passing context (like request IDs) across async operations
* Updating configuration without restarting applications
* Handling high load without slowing down the system
* Keeping logging failures from affecting the main application

## What I Learned

Building this helped me understand:

* How to think about **thread safety**
* How systems maintain **stability under pressure**
* How configuration can be updated safely
* How different parts of a system stay **independent but connected**
* Why observability is important in real-world applications

## Here is how I want to sum up my experience...

SmartLogger is not about creating “another logging library.”

It’s about learning how systems behave when:

* load increases
* failures happen
* multiple things run at the same time

> It helped me move from ***using systems*** to ***understanding how they are built***.

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
