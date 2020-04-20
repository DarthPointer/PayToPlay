# Welcome to PayToPlay repo!

This is a brand-new KSP reliability mod designed to  ~~cause some explosions~~ make you pay for engine maintainance. The idea is to have an easy-to-use mod that decreases parts' cost as they are being used and make them fail if you don't repair them. As for now it is an early WIP stage, the mod doesn't do much at the moment.

## What is present

Two engine reliability settings are available in editor. One for rated burn time and another for ignitions. They can be chosen within configurable limits. Adding burn time/ignition increases mass and cost of the egine. As you ignite and burn your enignes, they consume virtual weightless resources (visible in PAW and vessel resource tab). The resources are individual per engine (flow and transfer settings are copied from solid fuel). These resources take some of the cost of the engine, making it cheaper if you recover it.
In case of KCT to-storage recovery you recover craft with the same engine conditions, refill the resources in PAW to make it ready for the next flight.
This system is deeply configurable, see (the only) patch for "Twich" engine. Copypaste the module definition for other engines and tweak the numbers as you wish.

## Warning

To keep engine condition untouched on KCT recovery increasing reliability will not increase amount of "reliability" in the engine. Make sure you fill them manually before launching!

## Some regards

I'd like to thank kerbalism contributors team, their open source and advice have been essential to let me code the plugin.
