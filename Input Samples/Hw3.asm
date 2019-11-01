; Yuval Alfassi		318401015
; Ariel Nipomniashchi 	318261468
; We used the ****subroutine**** convention


.orig x3000

JSR NumberScrambleSolver

Halt

GetNum_Mem1 .fill #0
GetNum_Mem2 .fill #0
GetNum_Mem3 .fill #0
GetNum_Mem4 .fill #0
GetNum_Mem5 .fill #0
GetNum_Mem6 .fill #0
GetNum_Mem7 .fill #0
GetNum			; R0 <- Input num
	ST R7, GetNum_Mem7 
	ST R6, GetNum_Mem6 
	ST R5, GetNum_Mem5 
	ST R4, GetNum_Mem4 
	ST R3, GetNum_Mem3 
	ST R2, GetNum_Mem2 
	ST R1, GetNum_Mem1
	
	LD R3, NewLineAscii	; R3 <- ascii('\n')
	LD R4, ZeroAscii	; R4 <- ascii('0')

	AND R1, R1, #0		; R1 <- 0
InputNumLoop
	getc			; while ((R0 < getChar) != new line ascii)
	out			; print input num
	ADD R5, R0, R3		; if input == '\n' finish
	BRz Finish
	JSR MulR1Ten		; R1 <- R1 * 10
	ADD R6, R0, R4		; clear ascii value: minus ascii (zero)		
	ADD R1, R1, R6		; R1 <- R1 + input num
	Br InputNumLoop		; continue loop
Finish	
	ADD R0, R1, #0		; R0 <- R1

	LD R1, GetNum_Mem1 
	LD R2, GetNum_Mem2 
	LD R3, GetNum_Mem3 
	LD R4, GetNum_Mem4 
	LD R5, GetNum_Mem5
	LD R6, GetNum_Mem6 
	LD R7, GetNum_Mem7
	RET

MulR1Ten_Mem0 .fill #0
MulR1Ten_Mem2 .fill #0
MulR1Ten		; R1 <- R1 * 10
	ST R0, MulR1Ten_Mem0
	ST R2, MulR1Ten_Mem2

	AND R2, R2, #0		; sum <- 0
	ADD R0, R2, #-10	; i <- -10
MulR1TenLoop			; do
	ADD R2, R2, R1		;	sum += R1
	ADD R0, R0, #1		;	i++
	BRn MulR1TenLoop	; while i < 0
	ADD R1, R2, #0		; R1 <- sum

	LD R2, MulR1Ten_Mem2
	LD R0, MulR1Ten_Mem0
	RET

PrintNum_Mem0 .fill #0
PrintNum_Mem1 .fill #0
PrintNum_Mem2 .fill #0
PrintNum_Mem3 .fill #0
PrintNum_Mem4 .fill #0
PrintNum_Mem7 .fill #0
PrintNum			; Print num in R0
	ST R7, PrintNum_Mem7
	ST R4, PrintNum_Mem4
	ST R3, PrintNum_Mem3
	ST R2, PrintNum_Mem2
	ST R1, PrintNum_Mem1
	ST R0, PrintNum_Mem0
	
	JSR StoreNumInArray	; stores the number in R0 as an array at label NumArray
				; 57034 -> [-1, 5,7,0,3,4, -1]
	LEA R2, NumArrayEnd	; R2 <- pointer to the start of the array
	ADD R2, R2, #-1		; skip first -1
	LD R1, ZeroAscii	
	JSR Neg1		; R1 <- -(ascii('0'))
SkipToZero			; skip to the end of the number: [-1, 5, 7, 0, 3, 4, -1]
	LDR R4, R2, #0		; R4 <- *NumArray	
	BRn PrintNums		; if R4 < 0, you are at the end, start printing
	ADD R2, R2, #-1		; --NumArray
	BR SkipToZero
PrintNums
	ADD R2, R2, #1		; ++NumArray
	LDR R0, R2, #0		; R0 <- *NumArray
	BRn EndPrintNums
	ADD R0, R0, R1		; add to R0 the ascii value of zero
	out			; print R0
	BR PrintNums

EndPrintNums

	LD R0, PrintNum_Mem0
	LD R1, PrintNum_Mem1
	LD R2, PrintNum_Mem2
	LD R3, PrintNum_Mem3
	LD R4, PrintNum_Mem4
	LD R7, PrintNum_Mem7
	RET

ZeroAscii .fill #-48
NewLineAscii .fill #-10

NumArray .blkw #10
NumArrayEnd .fill #0
StoreNum_Mem0 .fill #0
StoreNum_Mem1 .fill #0
StoreNum_Mem2 .fill #0
StoreNum_Mem3 .fill #0
StoreNum_Mem4 .fill #0
StoreNum_Mem5 .fill #0
StoreNum_Mem7 .fill #0
StoreNumInArray			; stores the number in R0 as an array at label NumArray : 4563 - [0 .. 0, -1, 4,5,6,3, -1]. num : R0
	ST R0, StoreNum_Mem0
	ST R1, StoreNum_Mem1
	ST R2, StoreNum_Mem2
	ST R3, StoreNum_Mem3
	ST R4, StoreNum_Mem4
	ST R5, StoreNum_Mem5
	ST R7, StoreNum_Mem7
				; fill number with zeroes
	LEA R5, NumArrayEnd
	JSR Neg5		; R5 - pointer to the end of the array
	AND R1, R1, #0		; R1 = 0
	LEA R2, NumArray	; R2 - pointer to the beggining of the array	
InitilizeWithZeroes		; do
	STR R1, R2, #0		;	*arrayBegin <- 0
	ADD R2, R2, #1		;	arrayBegin++
	ADD R7, R5, R2		;
	BRnp InitilizeWithZeroes	; while arrayBegin != arrayEnd
	LD R1, NumArrayEnd	; arrayEnd <- 0

	LEA R5, NumArrayEnd	; R5 <- end of array.
	AND R3, R3, #0
	ADD R3, R3, #-1
	STR R3, R5, #0		; end of array: -1. [.. 4,6,2, -1]
	
	ADD R5, R5, #-1		; R5--

	AND R1, R1, #0
	ADD R1, R1, #10		; R1 <- 10. divisor.
StoreLoop			; do
	JSR EfficientDiv	; 	R2 <- num / 10, R3 <- num % 10
	STR R3, R5, #0		;	*array <- num % 10
	ADD R0, R2, #0		;	num <- num / 10
	ADD R5, R5, #-1		; 	array--
	AND R0, R0, R0		; 	apply (R0 = num) to condition code.
	BRp StoreLoop		; while num > 0
	
	AND R3, R3, #0
	ADD R3, R3, #-1
	STR R3, R5, #0		; start of of array: -1. [-1, 4,9,2 .. ]

	LD R7, StoreNum_Mem7
	LD R5, StoreNum_Mem5
	LD R4, StoreNum_Mem4
	LD R3, StoreNum_Mem3
	LD R2, StoreNum_Mem2
	LD R1, StoreNum_Mem1
	LD R0, StoreNum_Mem0
	RET

Rev_Mem4 .fill #0
Rev_Mem5 .fill #0
Rev_Mem6 .fill #0
Rev_Mem7 .fill #0
ReverseNumRepresantation		; reverses the represantation of the number in the array
	ST R4, Rev_Mem4
	ST R5, Rev_Mem5
	ST R6, Rev_Mem6
	ST R7, Rev_Mem7

	LEA R4, NumArray
SkipZeroes
	LDR R5, R4, #0			; skips the zeroes at the beggining of the number
	ADD R5, R5, #1
	BRz EncounteredMinusOne
	ADD R4, R4, #1
	BR SkipZeroes
EncounteredMinusOne			; num: [0..0, -1, 5,7,3, -1]
	LEA R5, NumArrayEnd		; R4 - left side, R5, right side
KeepSwapping				; do
	ADD R4, R4, #1			; 	leftDigitPointer++
	ADD R5, R5, #-1			; 	rightDigitPointer--
	LDR R6, R4, #0			; 	tempLeft <- *leftDigitPointer
	LDR R7, R5, #0			; 	tempRight <- *rightDigitPointer
	STR R6, R5, #0			; 	*rightDigitPointer <- tempLeft
	STR R7, R4, #0			; 	*leftDigitPointer <- tempRight

	ADD R6, R5, #0
	JSR Neg6
	ADD R6, R6, R4
	BRn KeepSwapping		; while (leftDigitPointer < rightDigitPointer)

	LD R7, Rev_Mem7
	LD R6, Rev_Mem6
	LD R5, Rev_Mem5
	LD R4, Rev_Mem4
	RET

EnterRowSize 	.stringz "Enter the board row size:\n"
EnterColSize 	.stringz "Enter the board col size:\n"
EnterBoard 	.stringz "Enter the board:\n"
NumberToSearch	.stringz "Enter number to search:\n"
Func_Mem0 .fill #0
Func_Mem1 .fill #0
Func_Mem2 .fill #0
Func_Mem7 .fill #0
Row 	.fill #0
Col 	.fill #0
Number 	.fill #0
NumberScrambleSolver
	ST R0, Func_Mem0
	ST R1, Func_Mem1
	ST R2, Func_Mem2
	ST R7, Func_Mem7

	LEA R0, EnterRowSize
	puts			; Print "Enter the board row size:"
	JSR GetNum
	ST R0, Row		; Store at label Row the result

	LEA R0, EnterColSize
	puts			; Print "Enter the board col size:"
	JSR GetNum
	ST R0, Col		; Store at label Col the result

	LEA R0, EnterBoard	
	puts			; Print "Enter the board:"
	JSR GetBoardASInput	; get the board values from the user

EnterNumberToSearch
	LEA R0, NumberToSearch
	puts			; Print "Enter number to search:"
	JSR GetNum
	ST R0, Number		; Store the number to search at label Number
	JSR StoreNumInArray
	JSR ReverseNumRepresantation

	JSR FindNum		; find the occurence of the number at the board
	
	Br EnterNumberToSearch	; infinite loop. always searches

	LD R7, Func_Mem7
	LD R2, Func_Mem2
	LD R1, Func_Mem1
	LD R0, Func_Mem0
	RET

MaxBoardSize .fill #20
GetBoardASInput_Mem0 .fill #0
GetBoardASInput_Mem1 .fill #0
GetBoardASInput_Mem2 .fill #0
GetBoardASInput_Mem3 .fill #0
GetBoardASInput_Mem5 .fill #0
GetBoardASInput_Mem6 .fill #0
GetBoardASInput_Mem7 .fill #0
GetBoardASInput			; gets the board data as input
	ST R0, GetBoardASInput_Mem0
	ST R1, GetBoardASInput_Mem1
	ST R2, GetBoardASInput_Mem2
	ST R3, GetBoardASInput_Mem3
	ST R5, GetBoardASInput_Mem5
	ST R6, GetBoardASInput_Mem6
	ST R7, GetBoardASInput_Mem7

	LD R6, ZeroAscii	; R6 <- -ascii('0')

	LD R0, Row		; R0 <- row size
	LD R1, Col		; R1 <- col size
	JSR EfficientMul	
	JSR Neg2		; R2 <- -(Row * Col)
	AND R1, R1, #0		; initilize R1 to zero.
GetBoardLoop
	ADD R5, R1, R2		; if (R1 < Row * Col) end loop
	BRzp EndLoop
	getc			; get number from user
	out
	ADD R0, R0, R6		; decrement from the input the ascii value of zero
	JSR GetAddressFromNum	; R1 - the number index. gets the address at register three
	STR R0, R3, #0		; store the number at the address at the table
	ADD R1, R1, #1		; R1++
	BR GetBoardLoop
EndLoop	
	LD R0, NewLineAscii
	JSR Neg0
	out			; print a new line
	LD R7, GetBoardASInput_Mem7
	LD R6, GetBoardASInput_Mem6
	LD R5, GetBoardASInput_Mem5
	LD R3, GetBoardASInput_Mem3
	LD R2, GetBoardASInput_Mem2
	LD R1, GetBoardASInput_Mem1
	LD R0, GetBoardASInput_Mem0
	RET

FromNum_Mem0 .fill #0
FromNum_Mem1 .fill #0
FromNum_Mem2 .fill #0
FromNum_Mem7 .fill #0
GetAddressFromNum		;input R1 - (row * ColSize + col). output: address - R3
	ST R0, FromNum_Mem0
	ST R1, FromNum_Mem1
	ST R2, FromNum_Mem2
	ST R7, FromNum_Mem7

	ADD R0, R1, #0		; R0 <- (row * ColSize + col)
	LD R1, Col		; R1 <- ColSize
	JSR EfficientDiv	; R2 <- row, R3 <- col
	ADD R1, R2, #0		; R1 <- row index
	ADD R2, R3, #0		; R2 <- col index
	JSR GetAddress		; gets the address at row, col

	LD R7, FromNum_Mem7
	LD R2, FromNum_Mem2
	LD R1, FromNum_Mem1
	LD R0, FromNum_Mem0
	RET
	
GetAddress_Mem1 .fill #0
GetAddress_Mem2 .fill #0
GetAddress_Mem4 .fill #0
GetAddress_Mem5 .fill #0
GetAddress_Mem6 .fill #0
GetAddress_Mem7 .fill #0
GetAddress			; input: R1 - row index, R2 - col index. output: R3 - address at board
	ST R1, GetAddress_Mem1
	ST R2, GetAddress_Mem2
	ST R4, GetAddress_Mem4
	ST R5, GetAddress_Mem5
	ST R6, GetAddress_Mem6
	ST R7, GetAddress_Mem7


	LD R3, BoardAddress	; R3: the address of the board
	LD R4, MaxBoardSize	; R4: the max size of the board - 20

	JSR Neg1		; R1 <- -(row index)

AddRows
	AND R1, R1, R1		; apply R1 to cc
	BRzp EndAddRows		; if R5 isn't negative, jump to label EndAddRows
	ADD R3, R3, R4		; add to the address the board size: R3 += 20
	ADD R1, R1, #1		; promote R5 by one
	BR AddRows
EndAddRows
	ADD R3, R3, R2		; add colomne reminder
	
	LD R7, GetAddress_Mem7
	LD R6, GetAddress_Mem6
	LD R5, GetAddress_Mem5
	LD R4, GetAddress_Mem4
	LD R2, GetAddress_Mem2
	LD R1, GetAddress_Mem1
	RET

BoardAddress .fill Board

FindNum_Mem0 .fill #0
FindNum_Mem1 .fill #0
FindNum_Mem2 .fill #0
FindNum_Mem3 .fill #0
FindNum_Mem4 .fill #0
FindNum_Mem5 .fill #0
FindNum_Mem6 .fill #0
FindNum_Mem7 .fill #0
NotFoundString .stringz "The number was not found in the board\n"
FindNum				; Checks for an occurrence of the number in the board
	ST R0, FindNum_Mem0
	ST R1, FindNum_Mem1	; for i = 0 to Row do
	ST R2, FindNum_Mem2	; for j = 0 to Col do
	ST R3, FindNum_Mem3	;	if FindNumAtCell(i,j) == 1
	ST R4, FindNum_Mem4	;		the number is found
	ST R5, FindNum_Mem5
	ST R6, FindNum_Mem6
	ST R7, FindNum_Mem7

	LD R1, Row
	JSR Neg1		; R1 <- -Row
	LD R2, Col
	JSR Neg2		; R2 <- -Col
	AND R3, R3, #0
	ADD R3, R3, #-1		; i <- -1. R3 : i

RowLoop
	ADD R3, R3, #1		; i++
	ADD R5, R3, R1		; if rowIndex >= Row
	BRzp NumNotFound	;	goto number is not found
	AND R4, R4, #0
	ADD R4, R4, #-1		; j <- -1. R4: j
ColLoop				
	ADD R4, R4, #1		; j++
	ADD R5, R4, R2		; if colIndex >= Col
	BRzp RowLoop		;	goto OuterLoop (i's loop)
	JSR FindNumAtCell	; Try find The number at the board. found: R0 = 0. not found: R0 = 1
	AND R0, R0, R0		; apply flag (R0) to condition code
	BRz EndSession		; (R0 == 0) => the number was found
	BR ColLoop	
		
NumNotFound
	LEA R0, NotFoundString
	puts
EndSession
	LD R7, FindNum_Mem7
	LD R6, FindNum_Mem6
	LD R5, FindNum_Mem5
	LD R4, FindNum_Mem4
	LD R3, FindNum_Mem3
	LD R2, FindNum_Mem2
	LD R1, FindNum_Mem1
	LD R0, FindNum_Mem0
	RET

AtCell_Mem1 .fill #0
AtCell_Mem2 .fill #0
AtCell_Mem4 .fill #0
AtCell_Mem5 .fill #0
AtCell_Mem7 .fill #0
FindNumAtCell			; R3- row, R4 - col. output: if the number is found at the cell: R0 = 0, print places. else print nothing, R0 = 1
	ST R1, AtCell_Mem1
	ST R2, AtCell_Mem2
	ST R4, AtCell_Mem4
	ST R5, AtCell_Mem5
	ST R7, AtCell_Mem7

	AND R0, R0, #0
	ADD R0, R0, #1

	ADD R1, R3, #0		; R1 <- row: for subroutine call
	ADD R2, R4, #0		; R2 <- col: for subroutine call

	AND R4, R4, #0
	ADD R5, R4, #1
	JSR DirectionTryFind		; R4 = 0, R5 = 1
	AND R0, R0, R0
	BRz End

	ADD R5, R5, #-2
	JSR DirectionTryFind		; R4 = 0, R5 = -1
	AND R0, R0, R0
	BRz End

	ADD R4, R4, #-1
	JSR DirectionTryFind		; R4 = -1, R5 = -1
	AND R0, R0, R0
	BRz End

	ADD R4, R4, #2
	JSR DirectionTryFind		; R4 = 1, R5 = -1
	AND R0, R0, R0
	BRz End

	ADD R5, R5, #1
	JSR DirectionTryFind		; R4 = 1, R5 = 0
	AND R0, R0, R0
	BRz End

	ADD R5, R5, #1
	JSR DirectionTryFind		; R4 = 1, R5 = 1
	AND R0, R0, R0
	BRz End

	ADD R4, R4, #-2
	JSR DirectionTryFind		; R4 = -1, R5 = 1
	AND R0, R0, R0
	BRz End

	ADD R5, R5, #-1
	JSR DirectionTryFind		; R4 = -1, R5 = 0
	AND R0, R0, R0
	BRz End
End
	LD R7, AtCell_Mem7
	LD R5, AtCell_Mem5
	LD R4, AtCell_Mem4
	LD R2, AtCell_Mem2
	LD R1, AtCell_Mem1
	RET

NumArrayEndAddress .fill NumArrayEnd
Direction_Mem1 .fill #0
Direction_Mem2 .fill #0
Direction_Mem3 .fill #0
Direction_Mem4 .fill #0
Direction_Mem5 .fill #0
Direction_Mem6 .fill #0
Direction_Mem7 .fill #0
R0_Temp .fill #0
RowTemp .fill #0
ColTemp .fill #0
DirectionTryFind			;  R1 - row cell, R2 - col cell, R4 - row jumping, R5 - col jumping. output: R0 = 0 if found, and print results, print nothing otherwse, R0 = 1
	ST R1, Direction_Mem1
	ST R2, Direction_Mem2
	ST R3, Direction_Mem3
	ST R4, Direction_Mem4
	ST R5, Direction_Mem5
	ST R6, Direction_Mem6
	ST R7, Direction_Mem7

	LD R0, NumArrayEndAddress
	ADD R0, R0, #-1			; R0 points to the beggining of the number
KeepMatching
	LDR R6, R0, #0			; R6 <- the number's digit
	JSR Neg6			; negate for comparison
	JSR GetAddress			; gets the address of the cell's num. puts it at register three. R3 <- &board[R1,R2]
	LDR R7, R3, #0			; gets the number at the cell
	ADD R7, R7, R6			; if (board[i,j] != digitArray[k])
	BRnp MatchNotFound		;	match not found
	
	ADD R0, R0, #-1			; increment number's digit array
	LDR R7, R0, #0			; if the num's digit is -1 (the end of the num) then
	BRn FoundMatch			; 	a match was found

	ST R0, R0_Temp
	ADD R1, R1, R4			; increment row
	ADD R2, R2, R5			; increment col
	JSR RowAndColValid		; if (row, col) position isn't valid
	And R0, R0, R0			
	BRz MatchNotFound		;	a match was not found
	LD R0, R0_Temp
	BR KeepMatching			; keep trying the match
FoundMatch
	LD R5, Direction_Mem5
	LD R4, Direction_Mem4
	LD R2, Direction_Mem2
	LD R1, Direction_Mem1
	JSR PrintLocations
	AND R0, R0, #0
	BR EndSubroutine
MatchNotFound
	AND R0, R0, #0
	ADD R0, R0, #1
	BR EndSubroutine
EndSubroutine
	LD R7, Direction_Mem7
	LD R6, Direction_Mem6
	LD R5, Direction_Mem5
	LD R4, Direction_Mem4
	LD R3, Direction_Mem3
	LD R2, Direction_Mem2
	LD R1, Direction_Mem1
	RET
RowAddress .fill Row
ColAddress .fill Col
RowCol_Mem5 .fill #0
RowCol_Mem6 .fill #0
RowCol_Mem7 .fill #0
RowAndColValid			; R1 - row, R2 - col. checks whether their location is valid. R0 != 0: Valid, R0 = 0 - not valid
	ST R5, RowCol_Mem5
	ST R6, RowCol_Mem6
	ST R7, RowCol_Mem7

	AND R1, R1, R1		; if row < 0
	BRn NotValid		; 	return NotValid
	AND R2, R2, R2		; if col < 0
	BRn NotValid		;	return NotValid
	LDI R5, RowAddress
	JSR Neg5
	LDI R6, ColAddress
	JSR Neg6
	ADD R5, R1, R5		; if (row >= RowLength)
	BRzp NotValid		;	return NotValid
	ADD R6, R2, R6		; if (col >= ColLength)
	BRzp NotValid		; 	return NotValid
	BR Valid		; otherwise - the result is valid
NotValid
	AND R0, R0, #0
	BR ReturnValidation
Valid
	AND R0, R0, #0
	ADD R0, R0, #1
	BR ReturnValidation

ReturnValidation
	LD R7, RowCol_Mem7
	LD R6, RowCol_Mem6
	LD R5, RowCol_Mem5
	RET

Smaller .fill #60
Bigger 	.fill #62
Comma 	.fill #44
NewLine .fill #10
NumberToFindAddress .fill Number
TheNumberWasFoundAt .stringz "The number was found at:\n"
PrintLocations_Mem0 .fill #0
PrintLocations_Mem1 .fill #0
PrintLocations_Mem2 .fill #0
PrintLocations_Mem6 .fill #0
PrintLocations_Mem7 .fill #0
PrintLocations			; R1 - starting row cell, R2 - starting col cell, R4 - row jumping, R5 - col jumping.
	ST R0, PrintLocations_Mem0
	ST R1, PrintLocations_Mem1
	ST R2, PrintLocations_Mem2
	ST R6, PrintLocations_Mem6
	ST R7, PrintLocations_Mem7

	LEA R0, TheNumberWasFoundAt
	puts			; print "The number was found at:\n"

	LD R6, NumArrayEndAddress
	ADD R6, R6, #-1
PrintNextCell
	LDI R0, NumberToFindAddress
	JSR StoreNumInArray	; stores the number to memmory
	LDR R7, R6, #0
	ADD R7, R7, #1		; if the num's digit is -1 (the end of the num) then
	BRz EndedPrinting	;	end printing

	LD R0, Smaller		; print '<'
	out
	ADD R0, R1, #0		; print row
	ADD R0, R0, #1		; row is zero based - increment by one
	JSR PrintNum
	LD R0, Comma		; print ','
	putc
	ADD R0, R2, #0		; print col
	ADD R0, R0, #1		; col is zero based - increment by one
	JSR PrintNum
	LD R0, Bigger		; print '>'
	putc
	LD R0, NewLine
	out
	ADD R1, R1, R4		; row += row jumping
	ADD R2, R2, R5		; col += col jumping
	ADD R6, R6, #-1		; increment num's digit pointer by one
	BR PrintNextCell	
	
EndedPrinting
	LD R7, PrintLocations_Mem7
	LD R6, PrintLocations_Mem6
	LD R2, PrintLocations_Mem2
	LD R1, PrintLocations_Mem1
	LD R0, PrintLocations_Mem0
	RET

;/******************************************* Helper subroutines ****************************************/

MinusOne .fill #-1		; Minus One: -1

Neg0				; negates register number zero
	NOT R0, R0		; two's complement -> bitwise not plus one
	ADD R0, R0, #1
	RET

Neg1				; negates register number one
	NOT R1, R1		; two's complement -> bitwise not plus one
	ADD R1, R1, #1
	RET

Neg2				; negates register number two
	NOT R2, R2		; two's complement -> bitwise not plus one
	ADD R2, R2, #1
	RET

Neg5				; negates register number five
	NOT R5, R5		; two's complement -> bitwise not plus one
	ADD R5, R5, #1
	RET

Neg6				; negates register number six
	NOT R6, R6		; two's complement -> bitwise not plus one
	ADD R6, R6, #1
	RET

;/******************************************* EfficientDiv ****************************************/

Div_Mem7 .fill #0		; stores the value of register 7 in subroutine Div

EfficientDiv			; R2 <- R0 / R1, R3 <- R0 mod R1
	ADD R1, R1, #0		; R1 - the denominator
	BRz ZERO_DENOMINATOR	; denominator == 0
	BRp POSITIVE_DENOMINATOR; denominator > 0
	BRn NEGATIVE_DENOMINATOR; denominator < 0

ZERO_DENOMINATOR 		; denominator == 0
	ADD R2, R1, #-1		; result = -1
	ADD R3, R1, #-1		; reminder = -1
	RET
POSITIVE_DENOMINATOR
	ADD R0, R0, #0
	BRp POSITIVE_POSITIVE	; nominator and denominator positive
	BRz ZERO_RESULT		; nominator is zero
	BRn NEGATIVE_POSITIVE	; nominator negative, denominator positive
NEGATIVE_DENOMINATOR
	ADD R0, R0, #0
	BRp POSITIVE_NEGATIVE	; nominator positive, denominator negative
	BRz ZERO_RESULT		; nominator is zero
	BRn NEGATIVE_NEGATIVE	; nominator and denominator negative

POSITIVE_POSITIVE		; nominator and denominator positive - just multiply
	ST R7, Div_Mem7
	JSR Divide
	LD R7, Div_Mem7
	RET
ZERO_RESULT			; nominator is zero
	AND R2, R2, #0		; result = 0
	AND R3, R3, #0		; reminder = 0
	RET
NEGATIVE_POSITIVE		; nominator negative, denominator positive
	ADD R0, R0, #0		; no-op
POSITIVE_NEGATIVE		; nominator positive, denominator negative
	ADD R0, R0, #0		; no-op
NEGATIVE_NEGATIVE		; nominator and denominator negative
	LD R2, MinusOne
	LD R3, MinusOne		; result: -1 in both answers.
	RET

Divide_Mem0 .fill #0		; stores the value of register 0 in subroutine Divide
Divide_Mem1 .fill #0		; stores the value of register 1 in subroutine Divide
Divide_Mem4 .fill #0		; stores the value of register 4 in subroutine Divide
Divide_Mem5 .fill #0		; stores the value of register 5 in subroutine Divide
Divide_Mem6 .fill #0		; stores the value of register 6 in subroutine Divide
Divide_Mem7 .fill #0		; stores the value of register 7 in subroutine Divide

Divide				; R2 <- R0 / R1, R3 <- R0 mod R1, R0, R1 are positive
				; R0: m
				; R1: n
				; R2: div
				; R3: res
				; R4: t1
				; R5: t2
				; R6: t3
	ST R0, Divide_Mem0
	ST R1, Divide_Mem1
	ST R4, Divide_Mem4	; store used registers in memmory
	ST R5, Divide_Mem5
	ST R6, Divide_Mem6
	ST R7, Divide_Mem7
	
	AND R2, R2, #0		; div <- 0
	ADD R3, R2, R0		; res <- m

WhileResGreaterN
	JSR Neg1
	ADD R7, R3, R1
	BRn FinishBigWhile	; while res - n >= 0 do
	JSR Neg1
	
	AND R4, R4, #0		; t1 <- 0
	AND R5, R5, #0		; t2 <- 0
	AND R6, R6, #0		; t3 <- 0

	ADD R4, R4, #1		; t1 <- 1
	ADD R5, R5, R1		; t2 <- n
	ADD R6, R5, R5		; R6 <- n << 1
	
InnerWhile
	JSR Neg6
	ADD R7, R3, R6		; while res - t3 > 0
	BRnz FinishInnerWhile			
	JSR Neg6
	ADD R4, R4, R4		;	t1 <- t1 << 1
	ADD R5, R5, R5		;	t2 <- t2 << 1
	ADD R6, R6, R6		;	t3 <- t3 << 1
	BR InnerWhile
FinishInnerWhile

	ADD R2, R2, R4		; div <- div + t1
	JSR Neg5
	ADD R3, R3, R5		; res <- res - t2
	Br WhileResGreaterN	; goto while

FinishBigWhile
	LD R7, Divide_Mem7
	LD R6, Divide_Mem6
	LD R5, Divide_Mem5
	LD R4, Divide_Mem4	; reload the values of the used registers from memmory
	LD R1, Divide_Mem1
	LD R0, Divide_Mem0
	RET


Mul_Mem7 .fill #0		; memmory for register 7 in subroutine EfficientMul


;/**********************************  EfficientMul  ****************************************/

EfficientMul			; Mul FUNCTION:   R2 <- R1 * R0 
	ADD R1, R1, #0
	BRz ZERO_MULTIPLIER
	ADD R0, R0, #0		; in case one of the multipliers is 0: the result is zero
	BRz ZERO_MULTIPLIER
	
	ADD R1, R1, #0
	BRn ERROR		; otherwise, if one of the multipliers is negative: the result is minus one
	ADD R0, R0, #0
	BRn ERROR
	
	ST R7, Mul_Mem7
	JSR Mul			; both elements are positive. multiply efficiently.
	LD R7, Mul_Mem7
	RET

ZERO_MULTIPLIER
	AND R2, R2, #0		; zero * _ = 0. => R2 <- 0
	RET

ERROR
	LD R2, MinusOne		; neg * _ -> Error => R2 <- -1
	RET


Mul_Mem0 .fill #0		; memmory for register 0 in subroutine EfficientMul
Mul_Mem3 .fill #0		; memmory for register 3 in subroutine EfficientMul

Mul				; Mul FUNCTION:   R2 <- R1 * R0 
				; a: R0, b: R1, c: R2, i: R3
	ST R0, Mul_Mem0		; load value of register 0
	ST R3, Mul_Mem3		; load value of register 3
	AND R2, R2, 0		; c <- 0
	ADD R3, R2, #-16	; i <- -16
MulLoop				; do
	ADD R2, R2, R2		; 	c <- c << 1
	ADD R0, R0, #0		
	BRzp ContLoop		; 	if a < 0 then
	ADD R2, R2, R1		;		c <- c + b
ContLoop
	ADD R0, R0, R0		; 	a <- a << 1
	ADD R3, R3, #1		; 	i <- i + 1
	BRnp MulLoop		; while i < 0

	LD R3, Mul_Mem3		; store value of register 3
	LD R0, Mul_Mem0		; store value of register 0
	RET

Board .blkw #400

.end