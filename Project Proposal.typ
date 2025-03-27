#let projectname = [Culture Miniature];
#let groupname = [GSND Abstract Workshop];

#set par(linebreaks: "optimized");

#{
	set align(center);
	par[
		#text(size: 20pt, weight: "bold")[
			Project Proposal for _#(projectname)_
		]
	];
	v(-1em);
	par[_#(groupname)_];
}

#{
	set align(center);
	show table.cell.where(y: 0): set text(weight: "bold");
	table(
		stroke: none,
		columns: 3,
		table.hline(stroke: 1pt),
		table.header[Name][NUID][Role],
		table.hline(stroke: 0.5pt),
		[Nianyi Wang], [002306100], [Technical artist],
		[Yichi Zhang], [002306795], [Client developer],
		[Zhuowen Song], [002345844], [Mechanism designer],
		table.hline(stroke: 1pt),
	);
}

#show heading.where(level: 1): set align(center);
#show heading.where(level: 1): set block(inset: (top: 0.2em, bottom: 0.5em));

= Quick Info

#{
	set align(center);
	show table.cell.where(x: 0): set text(weight: "bold");
	table(
		columns: 2,
		stroke: 0.5pt,
		align: left,
		[Project name], [#projectname],
		[Group name], [#groupname],
		[Type], [PCG demo],
	);
}

= One-sentence Summary

Simulation of the evolution of the civilization of a fictional intellectual species on a fictional planet.

= Theme

All of the planet's mesh, terrain and the cultures of the civilization will be procedurally generated.
To match the _Where Physical Meets Digital_ theme, physics-based terrain formation algorithm like iterative water erosion will be applied after the initial noise-based terrain height map is out.
We will try to use every method we can find to make the terrain look more realistic.

= Vision

In the expected final prototype, the audience could see the formation of the planet, the appearance of every early culture, their expansion and collision, and how the world evolves to a global society which mimics the modern world.

We would like our audience to have the following experiences:

- Admiring:

	Watching the growth of a civilization should be straight-forwardly glamorous and worths admiring.
	We hope to achieve this effect with visual/acoustic renderings and maybe supports from the mechanism side.

- Belonging:

	Have you ever watched those kind of "physics simulation of 1,000 balls bouncing in a container" video?
	Those videos are stupid and time-wasting, yet many bored netizens are willing to place a mental bet on "which ball would last to the last second".
	We hope that our audience would develop a similar belongingness to one of the fictional civilizations while watching the simulation.

= Scope

- A standard set of savegames UI: "New world", "Save/Load", etc.
- At the beginning of each run, play an animation of the world creation.
- A timeline which the audience could play/pause/drag to affect the simulation's position/speed in time.
- It will _not_ be playable in any sense, as making it a game would take too much time, not really realistic for the scope of this project.
	The audience will only play the role of an observer of the world.
- To achieve the belonging experience, we'll probably implement a functionality that allows the audience to follow/keep track of a civilization (and give invisible buffs to it, dank psychology lol).

= Approaches

- We will likely only use Unity for this project.

- We lack artists in our team, either for visual or acoustic expression.
	Any help/guidance on these aspects would be appreciated.

= Timeline

- Mar 21: Project proposal

	We gave an early head start, so by this time we should already have the planet mesh generation algorithm done.

- Mar 28: Prototype 1

	Terrain generation and modification algorithm.
	The mechanism for civilization development should be drafted.

- Apr 4: Prototype 2

	Civilization development simulation.

- Apr 11: Prototype 3

	Refining interaction experience & writing up the deliverable documents.
	Probably also making a video for in-class showcasing & future portfolio usage.

- Apr 18: Final prototype

	Finishing & presentation.

= Division of Work

- _Yichi_ will be in charge of developing the client-side logics, such as civilization development and user interfaces.

- _Zhuowen_ will be in charge of designing the mechanism for civilization development, i.e. how the cultures will interact with each other, what exact actions can they do, how will they be displayed on the map.
	He will write a mechanism document.

- _Nianyi_ will be in charge of developing the visual presentation of the in-game elements, mostly including the planet generation stuffs, terrain modification algorithm and shader tricks.
	After this part of the job is done, he will move on to join in the client-side works.
	He will also make the materials for presentation/showcasing purposes.