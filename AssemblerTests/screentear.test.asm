// Compute: Validates the input of the mapped keyboard input.
// Usage: Flips the pixels on the screen on any key input.

(MOON_LOOP)
    @SCREEN
    D = A

    @FLIP_PIXELS
    M = D

    @KBD
    D = M

    @WHITE_MOON // Create White moon if nothing is pressed.
    D ; JEQ
    D = -1

(WHITE_MOON)
    @MAKE_COLOR
    M = D

// Draw Black or White Moons.
(DRAW_MOON)
    @FLIP_PIXELS
    D = M

    // Get keyboard value.
    @KBD
    D = D - A
    @MOON_LOOP // If value >= 0. goto moon_loop
    D ; JGE

    @MAKE_COLOR
    D = M
    @FLIP_PIXELS
    A = M // Get the value from memory.
    M = D // Store the new pixels in memory from data registry as the color.
    D = A + 1 //  Get next lines of pixels as D. Then repeat process.
    @FLIP_PIXELS
    M = D

@DRAW_MOON
0 ; JMP