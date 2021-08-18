// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)
//
// This program only needs to handle arguments that satisfy
// R0 >= 0, R1 >= 0, and R0*R1 < 32768.

// Put your code here.

// Setup
// @0
// D = A
// @R0
// M = D

// @0
// D = A
// @R1
// M = D

@0
D = A
@R2
M = D

// Loop
@index // Variable to hold the counter of the multiplier.
M = 0

// Logic
(loop)
    // Break Conditions
    @R0
    D = M
    @loopEnd
    D ; JLE // Jump if @R0 <= 0

    @R1
    D = M
    @loopEnd
    D ; JLE // Jump if @R1 <= 0

    @index
    D = M
    @R0
    D = D - M
    @loopEnd
    D ; JGE // Jump if i > than @R0

    // Multiply Memory
    @R0
    D = M
    @R1
    D = M
    @R2
    M = D + M

    @index
    M = M + 1 // i++

    // @R2
    // D = M

    // @R1
    // D = D + M

    // @R2
    // M = D

    // @R0
    // D = M - 1
    // M = D

    @loop
    0;JMP

(loopEnd)

// @R1
// M = 0

(end)
    @end
    0;JMP
