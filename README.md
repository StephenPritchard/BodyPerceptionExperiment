# Body Perception Experiment

## Macquarie University
## Department of Cognitive Science

This experiment investigates whether some participants seem to perceive the size of their body
differently to its actual size.
The proxy used in the experiment for testing a participant's perception of their own body size
is to have the participant walk between two poles. The aperture between the poles varies from 
trial to trial. The participant may swivel their body as they move between the poles depending
on their own body perception versus their perception of the aperture size.
Experiment analysis will compare a control cohort with the target cohort of participants,
e.g., the target cohort may include participants diagnosed with anorexia.

There are two text files included:

## instructions.txt
This file can be edited by the experimenter to alter the instructions screen presented to participants
without needing to modify the Unity build itself.

## parameters.txt
The parameters file lets the experimenter tweak various aspects of the experiment, such as the number
of trials, aperture sizes, and what is tracked.

## Virtual Reality and tracking
The experiment uses HTC Vive trackers (on shoulders and/or hips) to track body movement and orientation.
The position/orientation of the hand controllers and HMD are also tracked. Optionally real poles may
also be tracked by placing a tracker atop each pole. A calibration step will occur after executing the program
but prior to the experiment starting. For this step, the participant has to stand on the starting marker,
facing towards the end marker, with their hands raised and to the side, and legs apart (roughly a star-shaped body pose).
This pose allows the experiment to assign a role/body-position to each tracker.