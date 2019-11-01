; Yuval Alfassi		318401015
; Ariel Nipomniashchi 	318261468

.orig x3000
JSR EfficientMul
HALT
;-------------------------------------- EfficientMul

Mul_Mem7 .fill #0		; memmory for register 7 in subroutine EfficientMul

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

;-------------------------------------- Helper subroutines

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

;-------------------------------------- EfficientDiv

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
	
.end