# Welcome to PayToPlay repo!

This is a brand-new KSP reliability mod designed to  ~~cause some explosions~~ make you pay for engine maintenance. The idea is to have an easy-to-use mod that decreases parts' cost as they are being used and make them fail if you don't repair them. As for now it is an early WIP stage, the mod doesn't do much at the moment.

## What is present

Two engine reliability settings are available in editor. One for rated burn time and another for ignitions. They can be chosen within configurable limits. Adding burn time/ignition increases mass and cost of the egine. As you ignite and burn your enignes, they get colser to a failure. As the engine looses it's burn time, it gets cheaper.
In case of KCT to-storage recovery you recover craft with the same engines condition. Press "maintenance" button to restore them if you don't want them to have higer fail chances next flight.
This system is deeply configurable, see present patches. Copypaste the module definition for other engines and tweak the numbers as you wish.
As for now, there is no random failure time, all affected engines fali if they have run for maximum possible time (not to be confused with rated time) or get locked if out of ignitinos.

## Installation

Grab the latest release and make sure you have module manager (in order to patch the EngineDecay MODULEs into engines). The mod is built for KSP 1.8.1, tested with MM 4.1.3. Should work with ANY MM 3.x or 4.x though.


## Some regards

I'd like to thank kerbalism contributors team, their open source and advice have been essential to let me code the plugin.
