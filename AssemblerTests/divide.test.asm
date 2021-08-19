

// 60 / 12 = 5
// RAM[0] / RAM[1] = RAM[2]
// @index = RAM[2] Contains - Contains the result of the division.
// @remainder = RAM[3] - Contains the remainder of the result if any.
//
// if RAM[0] >= 0 || RAM[1] >= 0. goto faulted.
// if result >= 5.1 reduce to nearest whole integer.

// Setup
// @0 // Setting R2 as zero.
// D = A
// @R2
// M = D

@index // Set variable "index" as zero.
M = 0

@remainder // Set variable "remainder" as zero.
M = 0

(BEGIN)

    @R0 // Yoink the value in memory at RAM[0].
    D = M
    @END
    D ; JEQ // if val = 0. goto END.

    @R1 // Yoink the value in memory at RAM[1].
    D = M
    @END
    D ; JEQ // if val = 0. goto END.


(BEGIN_DIVIDE)
    @R1 // Get the lowest value in the set.
    D = M
    @R0
    D = M - D // Substract the lowest value with highest value.
    @END
    D ; JLT
    @R0
    M = D

    @index
    M = M + 1

    // Validate if the calculation can be executed within the data provider.
    // Example: 4 / 12 = 0 remainder 4.
    @R0
    D = M
    @R1
    D = D - M
    @COMPLETED_WITH_REMAINDER
    D ; JLT

    @R0
    D = M
    @END
    D ; JEQ // End program if RAM[0] <= 0.

    @BEGIN_DIVIDE
    0 ; JMP

(COMPLETED_WITH_REMAINDER)
// Set result of division.
@R0
D = M
@remainder
M = D
@END
    
// (FAULTED)
//     // If faulted, set RAM[2] value in memory as zero.
//     @0
//     D = A
//     @R2
//     M = D
//     @END

// (COMPLETED)
//     // If the calculation is completed.
//     // Set summary.
//     @index
//     D = M
//     @R2
//     M = D
//     @END

(END) // End of program.
    @END // Infinite loop.
    0 ; JMP