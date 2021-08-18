// Compute: R2 = RAM[0] * RAM[1]
// Usage: Multiplies to numbers together, and stores the sum in RAM[2].

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
