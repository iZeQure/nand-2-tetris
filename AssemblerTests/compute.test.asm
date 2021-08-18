// Compute RAM[1] = 1 + 2 .. n
// Usage: Put a number (n) in RAM[0].

@R0
D = M // Grab the value of RAM[0].

@n // The number.
M = D // n = RAM[0]

@i // The index.
M = 1 // Set index to value of 1.

@sum // The summary.
M = 0 // Summary is 0, yet isn't computed.

(LOOP) // Main Loop.
    @i
    D = M

    @n
    D = D - M

    @STOP
    D ; JGT // Jump if i > n - STOP.

    // Summarize the result.
    @sum
    D = M

    @i
    D = D + M // Compute summary.

    @sum
    M = D // Update summary.

    @i
    M = M + 1 // Increment the index.

    @LOOP
    0 ; JMP

(STOP)
    // Display summary
    @sum
    D = M
    @R1
    M = D // Set RAM[1] as the summary.

(END)
    @END
    0 ; JMP