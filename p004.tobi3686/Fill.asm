// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed. 
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// Put your code here.

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
    A = M
    M = D
    D = A + 1
    @FLIP_PIXELS
    M = D

@DRAW_MOON
0 ; JMP