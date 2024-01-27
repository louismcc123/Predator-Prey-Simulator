# Predator-Prey Simulator

## Introduction
Watching and interacting with ecosystem models is an accessible and interesting way to demonstrate natural selection and the balance of populations in an ecosystem. Many factors affect this balance and the Lotka-Volterra equations from Alfred J. Lotka (1925) and Vito Volterra (1931) that I studied during my undergraduate degree in mathematics demonstrate how predator-prey relationship is key to this equilibrium. Using this, I wanted to make a model that shows how the variation of an animal’s attributes determine its behaviors and ultimately its survival. 

## Inspiration
My inspiration to develop the game came from studying autonomous and intelligent game characters. I began thinking about how I could implement multiple non-playable characters (NPC’s) in a world and see how they interact with the world and each other. In researching the topic, I found that, unsurprisingly, I was not the first person to develop the idea. Many developers have built their own versions, for example ThinMatrix (2018) developed and published Equilinox, a nature simulation game in which the player builds complex ecosystems in a world with a variety of landscapes and wildlife. Youtubers such as Primer (Simulating Natural Selection, 2018) and Sebastian Lague (Coding Adventure: Simulating an Ecosystem, 2019) have also shown examples of how this idea can be developed and heavily influenced my overall vision and goals for my game.

## The Model
The model is built through four main systems: the Environment, the Animals, Camera Controller, and User Interface.

### Environment
The Environment class manages the spawning of entities (animals, plants, trees), as well as containing a key method named ‘getRandomPosition’. This method produces a point in a random position on the Navigation Mesh (nav mesh). This is used to spawn entities in random positions in the world, and is also utilized in the animal’s movement, which will be discussed shortly. The Environment class is responsible for maintaining the population of plants and trees above certain values. This is particularly crucial since the rest if the food chain is reliant on enough plants to eat. The initial populations of all animals, plants and trees are declared through the Environment class, and this will eventually be adjustable by the player.

The terrain is a flat square tilemap that comprises of land and water tiles. I added Broken Vector’s Low Poly Rocks (2018) for a small amount of scenery around the island. 

### Animals
The game currently consists of two animal species; predators and prey. I have used VoxelGuy’s (2019) animal prefab designs for each. The simulation has separate classes for the prey and the predators, however both are similar in their design. The aim was to develop simplified behavior patterns using a finite state machine for the animals to imitate real animal survival needs and actions. 

Based on parameter attributes and surrounding objects/entities, the animal will switch state to decide on their next move. The parameter attributes of the animals include hunger, thirst, and reproductive urge (‘reproductiveUrge’), that all increase over time. Hunger and thirst are also inversely proportional to the animal’s speed. This was so that an energy or stamina parameter is in effect, without having to declare it as its own attribute.

### Death
If any live entity dies, a method ‘Die’ from the LiveEntity class is called, destroying the game object. Animals will die when their hunger or thirst reaches a value of 1, or when a predator kills them. Plants will die from either being eaten by an animal or being trampled by one. The plant is trampled if an animal collides with it without being in an Eat state.

### State Machine
I designed the animal state machines by implementing techniques used by Inexperienced Developer (2021) and Code Monkey (2020). The state machine utilizes the switch function to run different behavior logic based on the current state. 
The prey have five states: Wandering, Eat, Drink, Flee, and Reproduce. Each state has its own update method that defines the necessary conditions for transitioning to a different state. 

For example, if the prey is in the Wandering state, it will run the ‘UpdateWander’ method. This first checks whether the prey is in danger through a Boolean that confirms if a predator is in sight or not. If it is in danger it will transition to a Flee state. The danger flag overrides all other states the prey may be in to copy a real-life scenario. For example, even if an animal is hungry, it would still flee if facing certain death by a predator. This of course does not factor the complex energy systems of animals, but for the purposes of the simulation, is the most similar behavior to reality.

If the prey is not in danger, it will assess the hunger, thirst, and reproductive urge. If the hunger of the prey is greater than 15% and greater than or equal to the thirst and the reproductive urge, it will transition to Eat state. If the thirst is greater than hunger and reproductive urge, it will transition to the Drink state. And finally, if its reproductive urge is greater than hunger and thirst and the time since it last had a child is greater than 10 seconds, it will transition into the Reproduce state. 

This logic is near identical for the predators; however, the predators do not sense danger and so do not have a Flee state. In its current state, this game could have one class for both animals, however I felt it would be easier to modify the species individually in the future if they have separate classes. 

### The OnTriggerStay Function 
Both animal scripts contain an OnTriggerStay function that acts as their spatial awareness. This format is inpired by tutorials by Sykoo (Sykoo, 2017) and ReginTheSmith (ReginTheSmith, 2021). The method ‘is called once every physics update for every collider that is touching the trigger’ (Unity Technologies, 2022). The animals have a vision radius (private float visionRadius) that dictates how far their spatial awareness (trigger) is. Based on the animal’s current state, they will decide whether the collider is of interest to them and decide what to do next.

Taking a closer look at the OnTriggerStay function, we can see what it does for each animal. The animal will first determine what it is ‘seeing’. 

If the animal is seeing an entity (another animal or a plant) it will then determine what species the entity is and whether it is of interest.

In the event of a predator seeing an entity that is in its diet (prey) while in an Eat state, it will register that it has found an interest and will set its target position to chase the entity. When close enough, it will eat the food (prey) causing its hunger to be restored, the prey will die, and the predator will lose interest in food/eating. 

If a prey sees an entity that is a predator, it will know it is in danger, transition to a Flee state, and flee in the opposite direction. If it manages to evade the predator by getting farther away than its vision radius, it will no longer be in danger and will return to a Wandering state. If the predator catches it, it will die. 

If the entity is of the same species as the animal and the animal is in the Reproduce state, it knows it has found an interest and will begin assessing whether or not the entity is a suitable mate. To do this, the animal script calls on the entity’s script (Prey or Predator class). If the entity is also in a Reproduce state, the animal will check that they are opposite genders and if so, will set its target position to the position of the entity. When close enough to each other, the animals will reproduce.

### FindClosestWater and FindClosestPlant
Unlike reproduction or fleeing, if an animal is thirsty (in its Drink state) or hungry (in its Eat state), it will not use OnTriggerStay to find the required water or food. Instead, it will call on either the FindClosestWater or FindClosestPlant method. The FindClosestWater method finds all the water tiles in the scene and works out which water tile the animal is closest to. If the closest water tile is within the animal’s vision radius while in the Drink state, the animal will set that water tile as its target position and being moving towards it. If it manages to get close enough to the water, it will drink, resulting in the thirst resetting and the water will no longer be of interest. FindClosestPlant works the exact same way but is only utilized by prey when looking for plants to eat.

### Reproduction and Genetic Algorithm
The reproduction logic is a very simple genetic algorithm. In the event of two animals reproducing, we start by defining the time between conceptions (timeToNextChild) as equal to the sum of time and the cool down period (childCoolDown) of 15 seconds. This prevents the animals constantly mating or having too many children at once.

A child is born (instantiated) at the same position as the parents. The child’s hunger, thirst, and reproductive urge parameter will be set to zero, and there is a 50% chance of it being male or female. 

Now, the model would not be of much interest without introducing some version of evolution. When a child is born, there is a 5% chance for each of its parameters that a mutation occurs. If this happens, the value of the parameter which is inherited from the parent will increase or decrease a random amount between -1 and 1. If this doesn’t occur, the attribute will be inherited by one of the parents with an even probability. However, currently the ecosystem tends to reach extinction before any noticeable change in the animals has occurred. I will discuss ways in which this can be fixed in the Future Implications section.

### Movement 
Through Unity’s AI engine, I created a ‘walkable’ nav mesh, which gave the animals the ability to understand where they can and cannot reach. It also meant that they could find the most efficient path to their desired destination whilst avoiding obstacles (trees and water), since Unity’s nav mesh uses the A* algorithm to achieve this.

The desired destination (Vector3 targetPosition) is defined in the animal classes (Prey and Predator).

In order to move to the desired position, the animals use a Move method. This method is called in the Update function, so that the animal is moving towards an updated target position every frame. The animal will rotate towards the target using the Slerp method, which spherically interpolates between two vectors: the current quaternion and the target quaternion (Unity Technologies, 2022). The animal has a rotational speed (rotationSpeed) that defines how quickly the animal turns to look at the target position. After the rotation, the animal will move towards the target at its speed. The speed and rotational speed of the animal can evolve though generations as a result of the genetic algorithm.

If the animal has not found an interest, is not in danger, and/or is in the Wandering state, it will explore the world without a particular desired destination. In this case, it will use a ‘Wander’ method. This method sets a new random target position every second, calling the getRandomPosition method from the environment class. 

### Camera Controller
Currently, the camera controller is the main input manager for the game. With help a camera controller tutorial (Game Dev Guide, 2019), I built a camera system designed in a strategy game style. The controller manages movement, zoom, and rotation of the camera. There are two key methods used: ‘HandleMouseInput’, and ‘HandleMovementInput’. These methods manage the mouse and keyboard inputs, respectively. A key feature I wanted to include in the game was the ability to focus on an animal and have the camera follow its movement around the world. As shown in the tutorial, I added a follow transform to the camera rig and in the animal scripts include an ‘OnMouseDown’ function, declaring that if the animal is clicked on, the camera rig will follow the position of the animal.

### User Interface (UI)
Unfortunately, the user interface is yet to be completed. Currently the interface consists of four panels: a population panel, start/pause panel, the species (bottom) panel, and the attributes panel. The population panel has not been fully developed yet. However, this will show the current population of each species in the world. The start/pause panel has working start and pause buttons as well as a restart button that was developed with help from xxRafael Productions - Rafael Vicuna (2021). I will add a fast forward button to this so that the player can choose from a selection of speeds of the game. The design of the species panel at the bottom of the screen is based on Shamim Akhtar’s tutorial (2021). One of the issues I found with this was the game does not recognize when the mouse is over the UI and when it is not. This means that in its current state, trying to interact with the UI just results in moving the camera. To fix this I have attemped to create a boolean that declares where a raycast from the mouse hits the UI or not, and thus whether or not to control the camera. However, I have not yet managed this so once this issue has been resolved, the attribute panel will show up when an animal is selected (either in the species panel or in the world), showing the current parameter attribute values for that animal. The attributes panel is also incomplete and is not currently being used.

### Audio
I wanted the simulation to have calm relaxing music so downloaded a track by Dimitri Kovalchuk (2020) from the unity asset store. Once I had set up the audio source in the main menu, I wanted the music to continue playing when the player clicked play and started the game. To do this I used CatoDevs (2020) guidance to make sure the music did not stop or restart when a new scene was opened.

## Future Implementations:
This game is incomplete, and I have several implementations I intend on adding in the future.  
### Current Bugs and Flaws
The first challenge is to eliminate or minimize the bugs and flaws that currently exist. 

#### Movement
Currently, the animals have a jitter while they move and don’t appear to always follow the shortest path to their target destination. At times they will spin on the spot. This may be caused by the stopping distance on the nav mesh agent, however after trialing various distances, it has not appeared to solve the issue. The animals will also sometimes get stuck trying to walk across water or off the edge of the map. They should know from the nav mesh that they cannot go that way since it is a non-walkable area, so I have been unsuccessful in solving this so far. 

#### Camera
I would like to make the camera controller more user friendly. The rotation and zoom speeds are too fast and need to be limited so that the world doesn’t disappear out of view leaving the player lost. 

### Further Development
#### Genetic Algorithm
In the future I would like to make the genetic algorithm more elegant. This would involve building a more life-like algorithm that involves DNA and chromosomes that each animal has and will inherit from its parents through crossover functions. The motivation for this has originated from a thesis by Sjöberg et al. (2021), which is an in-depth view into most of the ideas comprised in this game.

#### Animals 
I may consider giving the animals some more attributes such as health and age. I think that, with a well-designed genetic algorithm, these could be very interesting to observe how the animals evolve these attributes over generations. It would also be an opportunity for the player to have more interaction and freedom in the game to play more of a god-like role. They could choose to include or remove certain attributes and could choose the values of the parameters. 
Another attribute that I think would be good to change is the field of view of the animals. Inexperienced Developer (2021) and Sebastian Lague (2015) have made tutorials in how this can be achieved that I would like to come up with my own version of. 
I would like to add more species to the game, turning it from a predator-prey model to a full-blown ecosystem. Attila Kiss and Gábor Pusztai (2022) developed a simulation that is similar in function to games such as Equilinox (ThinMatrix, 2018). Although these are more complex than I could realistically achieve on my own within a reasonable amount of time, there are many features that I take inspiration from and would like to try myself.
I also intend on adding audio and animation to the animals. It has been my intension to include these features throughout the development process so far but they have been lower priority than other features that have been essential.
I would also like to redesign the logic for the animals as the current model has performance issues when too many animals are in the scene. This is predominantly due to the OnTriggerStay function constantly checking the surroundings for every animal at once. I hope to utilise other functions to reach the same desired effect but with more efficiency.
#### Tile-based terrain
I would like to make more sophisticated level designs with random (procedural generated) tilemaps spawning at the start of the game (not the set tilemap I have made myself). I could also make different tiles for different scenes such as desert and ice scenes, with appropriate animal and plant species living there. It would be another option for the player to make the game that much more dynamic and customisable. 
#### User Interface
As previously mentioned, the UI is in early stages. The UI, once properly implemented, will be the main feature that converts the simulation into an interactable game. Once I have fixed the mouse input control issue, I intend on building an interface with the following features:
-	In-game statistics to display the current population of each animal and the average parameter attribute value of each species
-	Speed controls so the player can fast forward or slow down the simulation.
-	I would like the player to be able to click on a species in the UI and be able to customise the parameters of the species, before dragging and dropping them into the world 
-	Save button so that the player can come back to their game if they have managed to keep the ecosystem alive for a long time
#### Main Menu
With the help of Brackeys (2017) tutorial, I began making a simple main menu for the game. However, this was also a low-priority feature and so have not developed it beyond a play button that starts the simulation and an option menu that currently only allows the player to change the music volume. I would like to add more settings such as brightness, language, controls, etc. 

### Conclusion
The game is a sandbox-style simulation game in which a player can adapt an ecosystem and watch how the balance of populations react to the attribute changes. I am still in the process of adding more functionality to the UI, which will make a big difference to the player experience. There are also bugs and flaws in the game, particularly with the animal movements and behaviours, that would affect the player engagement. I intend on continuing the development to get these bugs fixed and optimize the overall performance and enjoyability of the game. These developments will involve a more advance genetic algorithm, more animal attributes, and a more diverse world. The simulation currently cannot perform with too many animals in the scene at once, so is not yet effective for large scale simulations. I am also interested in investigating how I can analyse the information from the simulation in a more informative way. If I could develop models that show how the agents interact with each other and the world (such as spawn positions, speed, etc.), I believe a lot could be learned and the model could be drastically improved by observing the results. 

## References
1.	Unity Technologies (2022) MonoBehaviour.OnTriggerStay(Collider). Available at: https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnTriggerStay.html (Accessed: 16/12/2022).
2.	Unity Technologies (2022) Quaternion.Slerp. Available at: https://docs.unity3d.com/ScriptReference/Quaternion.Slerp.html (Accessed: 17/12/2022).
3.	Sebastian Lague (2019) Coding Adventure: Simulating an Ecosystem. 10/06/2019. Available at: https://www.youtube.com/watch?v=r_It_X7v-1E&t=91s (Accessed: 17/12/2022).
4.	ThinMatrix (2018) Equilinox [Video game]. ThinMatrix (17/12/2022).
5.	Primer (2018) Simulating Natural Selection. 15/11/2018. Available at: https://www.youtube.com/watch?v=0ZGbIKd0XrM&t=109s (Accessed: 17/12/2022).
6.	Game Dev Guide (2019) Building a Camera Controller for a Strategy Game. 24/06/2019. Available at: https://www.youtube.com/watch?v=rnqF6S7PfFA (Accessed: 18/12/2022)
7.	Inexperienced Developer (2021) Create the MOST manageable AI State Machine using Unity Scriptable Objects [ Intermediate Tutorial]. 12/04/2021. Available at: https://www.youtube.com/watch?v=l90d4z5nVWI&t=2286s (Accessed: 18/12/2022)
8.	Sykoo (2017) Hunger, Thirst, Stamina & More! | C# Tutorials in Unity. 13/05/2017. Available at: https://www.youtube.com/watch?v=uud9d20ifnk (Accessed: 18/12/2022)
9.	ReginTheSmith (2021) Unity Ecosystem Tutorial #3: Predators And Prey.  06/11/2021. Available at: https://www.youtube.com/watch?v=nYyHg9ww8TU&list=PLd4sR6DjJe3HXy-wvO4EQrTto57gfWZUe&index=3 (Accessed: 18/12/2022)
10.	Sebastian Lague (2015) Field of view visualisation (E01). 26/12/2015. Available at: https://www.youtube.com/watch?v=rQG9aUWarwE (Accessed: 18/12/2022)
11.	CatoDevs (2020) Unity Tutorial: How to Seamlessly playing music between multiple scenes. 01/09/2020. Available at: https://www.youtube.com/watch?v=Xtfe5S9n4SI&list=PLQ2M0GzskxrodDn_sEF29o_Ff89kRcRsu&index=7 (Accessed: 18/12/2022)
12.	CatoDevs (2020) Unity Tutorial: How to change sound volume using a slider. 18/05/2020. Available at: https://www.youtube.com/watch?v=-xvoJ7Q4vw0&list=PLQ2M0GzskxrodDn_sEF29o_Ff89kRcRsu&index=2 (Accessed: 18/12/2022)
13.	Dimitri Kovalchuk (2020) Beneath the stars. Available at: https://assetstore.unity.com/packages/audio/music/orchestral/calm-melancholic-orchestral-purple-horizon-183167 (Downloaded: 18/12/2020).
14.	Sjöberg, A. et al. (2021) Simulating an Ecosystem. Bachelor’s Thesis in Computer Science and Engineering. CHALMERS UNIVERSITY OF TECHNOLOGY. UNIVERSITY OF GOTHENBURG. Available at: https://gupea.ub.gu.se/bitstream/handle/2077/69615/gupea_2077_69615_1.pdf?sequence=1&isAllowed=y (Accessed: 18/12/2020).
15.	Kiss, A., Pusztai, G. (2022) 'Using the Unity Game Engine to Develop a 3D Simulated Ecological System Based on a Predator–Prey Model Extended by Gene Evolution', Informatics 2022, 9, 9. Available at: https://doi.org/10.3390/informatics9010009 (Accessed 18/12/2022)
16.	Unity Technologies (2022) GameObject.FindGameObjectsWithTag. Available at: https://docs.unity3d.com/ScriptReference/GameObject.FindGameObjectsWithTag.html (Accessed: 18/12/2022).
17.	Brackeys (2017) START MENU in Unity. 29/11/2017. Available at: https://www.youtube.com/watch?v=zc8ac_qUXQY (Accessed: 18/12/2022)
18.	VoxelGuy (2019) 5 animated Voxel animals. 22/05/2019. Available at: https://assetstore.unity.com/packages/3d/characters/animals/5-animated-voxel-animals-145754 (Downloaded: 26/10/2022).
19.	Shamim Akhtar (2021) Implement Drag and Drop Item in Unity. 18/08/2021. Available at: https://faramira.com/implement-drag-and-drop-item-in-unity/ (Accessed: 18/12/2022).
20.	Code Monkey (2020) Simple Enemy AI in Unity (State Machine, Find Target, Chase, Attack). 08/01/2020. Available at: https://www.youtube.com/watch?v=db0KWYaWfeM&t=618s (Accessed: 18/12/2022)
21.	xxRafael Productions - Rafael Vicuna (2021) Unity - How to Reset / Reload Scene with C# (Restart Game Button Click, 2021) | Easy Tutorial. 12/12/2021. Available at: https://www.youtube.com/watch?v=TVSLCZWYL_E (Accessed: 18/12/2022)
22.	Lotka, A. (1925) Elements of Physical Biology. Williams and Wilkins. Available at: https://archive.org/details/elementsofphysic017171mbp (Accessed: 18/12/2022).
23.	Volterra, V. (1931) Lessons on the Mathematical Theory of Struggle for Life (Original: Leçons sur la théorie mathématique de la Lutte pour la vie). 
24.	Broken Vector (2018) Low Poly Rock Pack. 09/07/2018. Available at: https://assetstore.unity.com/packages/3d/environments/low-poly-rock-pack-57874 (Downloaded: 15/12/2022).
