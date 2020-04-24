# Welcome to PayToPlay repo!

This is a brand-new KSP reliability mod designed to  ~~cause some explosions~~ make you pay for engine maintenance. The idea is to have an easy-to-use mod that decreases parts' cost as they are being used and make them fail if you don't repair them. As for now it is an early WIP stage, the mod doesn't do much at the moment.

## What is present

Two engine reliability settings are available in editor. One for rated burn time and another for ignitions. They can be chosen within configurable limits. Adding burn time/ignition increases mass and cost of the egine. As you ignite and burn your enignes, they get colser to a failure (just running out of ignitions or a fail-while-running). Restoring engine's runtime takes money (and time if KCT is present).
In case of KCT to-storage recovery you recover craft with the same engines condition. Press "maintenance" button to restore them if you don't want them to have higer fail chances next flight.
This system is deeply configurable, see present patches. Copypaste the module definition for other engines and tweak the numbers as you wish.

## Installation

Grab the latest release and make sure you have module manager (in order to patch the EngineDecay MODULEs into engines). The mod is built for KSP 1.8.1, tested with MM 4.1.3. Should work with ANY MM 3.x or 4.x though.

## Other mods

#### Dependencies
- ModuleManager. Only MM 4.1.3 is tested but it is unlikely that 3.x and 4.x will cause problems.

#### Suggested 
- KCT: PayToPaly is designed to be used with KCT, it makes restoring recovered craft take time.

#### Supported
- Kerbalism: PayToPlay is provided a patch to disable engine reliability of kerbalism. Kerbalism has a lot of interesting features and it is unacceptable to be incompatible with it. Kerbalism has its own patch to do this but it is not released yet.

## Some regards

I'd like to thank kerbalism contributors team, their open source and advice have been essential to let me code the plugin.
