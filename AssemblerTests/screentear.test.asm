// Compute: Validates the input of the mapped keyboard input.
// Usage: Flips the pixels on the screen on any key input.

(MOON_LOOP)
    @SCREEN
    D = A

    @screenpixels
    M = D

    @KBD
    D = M

    @BLACK_MOON // Create White moon if nothing is pressed.
    D ; JEQ
    D = -1

(BLACK_MOON)
    @color
    M = D

// Draw Black or White Moons.
(DRAW_MOON)
    @screenpixels
    D = M

    // Get keyboard value.
    @KBD
    D = D - A
    @MOON_LOOP // If value >= 0. goto moon_loop
    D ; JGE

    @color
    D = M
    @screenpixels
    A = M // Get the value from memory.
    M = D // Store the new pixels in memory from data registry as the color.
    D = A + 1 //  Get next lines of pixels as D. Then repeat process.
    @screenpixels
    M = D

@DRAW_MOON
0 ; JMP