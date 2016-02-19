# REstate
StateMachine configured and accessed over HTTP REST API

###What is a state machine??

A State machine, in this specific case a Finite State Machine, is a model of computation that can represent a "machine" and all of its possible states and how each state is to be triggered.

[Wikipedia article which dives into theory here.](https://en.wikipedia.org/wiki/Finite-state_machine)

###What practical use is there?

A common example of a finite state machine is a traffic light. There are rules that dictate when and ain what order the light can change colors;this is modeled as a state machine.

Somewhat similarly in application development we often have to track states of processes such as business processes (Orders, Invoices, Leads, Issues, Work Orders, so on...).

###What benefit is there for a web based State Machine

I would be lying if I said I knew all of the possibilities, but there are likely a ton.

There are two particular use cases I am developing for in the initial product.

1. A platform independent, back-end configureable, and horizontally scalable workflow system. This has several benefits: 
  * HTTP REST based means ANY system capable of making HTTP calls can interact with the workflow and leave tracking state to system designed to do so rather than embedding logic that is oftten poorly maintained and difficult to follow and model. (Think case statements or n-layers of nested ifs)
  * Scaling independently is a breeze.
  * Since state is decoupled from the original usage scenario, outside processes can be allowed to contribute to state changes as Add-In processes without building complex rules to allow this.
  * ANY existing solution can add complex state tracking with minimal code changes.
2. Web UI state modelling and tracking.
  * UI designers and developers know just how complicated business driven UIs can get with nested and hierchical states. I would venture to say MOST UI bugs are handling properly these state changes back and forth and coding for all of the exceptions. Since Persistate is HTTP based, any web client can call out to the system and ask for current state and triggering transitions.
  
  


