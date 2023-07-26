# RPTU-MQTT-Coding-Challenge

Task Description:

1) Download and install the Unity Enginge(https://unity.com/de). Then create a new project called "RPTU-MQTT-Coding-Challenge".

2) Place a simple sphere (a ball) which can then move in the X,Y,Z coordinates. 
Furthermore implement a MQTT client in the Unity project(https://workshops.cetools.org/codelabs/CASA0019-unity-mqtt/index.html?index=..%2F..index#0) which subscribes to the topic "movement". 
On this MQTT topic messages are sent in the following format: (X,Y,Z) based on these values the sphere should move.

An example: (10,-5,4) this would mean that the sphere should move +10 on the X axis, -5 on the Y axis and +4 on the Z axis. Of course, each axis can have positive or negative values.

Other possible examples: (,3,) (-4,,) (,,8) if no values are given the value is automatically 0. In other words (,3,) = (0,3,0); (-4,,) = (-4,0,0);  and (,,8) = (0,0,8)

3) As soon as the sphere has moved in the game, it should send the information to the same MQTT server(https://mosquitto.org/), but with the topic "information": I have moved from (X: x1, Y: y1, Z: z1) to (X: x2, Y: y2, Z: z2).

Example: If the ball was at (5,5,5) before and it moves by (3,-7,8) the message to the information topic should be like this: I have moved from (X: 5, Y: 5, Z: 5) to (X: 8, Y: -2, Z: 13).

4) Finally, develop a CLI (with a language of your choice) that can connect to the MQTT server. This CLI should be able to send messages to the topic "movement" (in the mentioned format).
Additionally your CLI subscribes to the topic "information" and outputs the message it receives.
