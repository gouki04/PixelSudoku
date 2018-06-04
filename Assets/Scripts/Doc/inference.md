# 推论(Inference)

在数独中，推论是关于前提之间的相互作用的陈述，其前提是关于数独状态的陈述必须是真或假。 最常见的前提是特定单元(cell)具有特定的候选值。 当它们用于链(chain)或循环(loop)时，术语推论(inference)等同于链接(link)。

    In Sudoku, an inference is a statement concerning the interaction between premises, where a premise is a statement concerning the state of the Sudoku that must be either true or false. The most common type of premise is that a particular cell has a particular candidate value. When they are used in chains or loops, the term inference is equivalent to link.

有一些关于推论(inference)和暗示(implication)之间的区别的讨论。

    There is some discussion about the difference between inferences and implications.

有两种类型的推论。 强推论和弱推论:

1. 如果两个前提不可能都是假的，那么这两个前提之间是**强推论**.

2. 如果两个前提不可能都是真的，那么这两个前提之间是**弱推论**.

## 强推论

强推论（或链接(link)）的例子，其中两个前提不可能是假的，包括：

- The two candidate values of a bivalue cell.
- The common candidate value of a bilocal unit.
- A candidate value of a cell C, and that value in the group of cells consisting of all cells with that candidate value that share a house with C.

Note that all of these are also examples of weak inferences (see below). Many types of strongly linked premises can also be weakly linked, but this is not always the case. For example, consider this potential Unique Rectangle:

$$
\left(\begin{array}{cc}
(123)a & 0 & 0 \\
0 & 0 & 0 \\
(12)c & 0 & 0 \\
\end{array}\right)
\left(\begin{array}{cc}
0 & (12)b & 0 \\
0 & 0 & 0 \\
0 & (125)d & 0 \\
\end{array}\right)
$$

For the Sudoku to have a unique solution, either a must be 3 or d must be 5. Those two prmises are therefore strongly linked, since they cannot both be false. However, they are not weakly linked, as it is possible for both to be true.

Patterns with the term almost in their name, such as the Almost Locked Set, are very useful for forming strong inferences.

For two premises, named A and B, the following strong inference deductions can be made:

- If A is false, B is true.
- If B is false, A is true.

Strong inference is represented in most notation systems by an equal sign: -.

## Weak Inference

Examples of weak inferences (or links), where both premises cannot be true, include:

- Any two candidate values of a single cell.
- A candidate value of a cell C, and that value in any cell that shares a house with C.
- Any two disjoint subsets of candidate values of an Almost Locked Set, where the cells of the ALS having those values as candidates are either the same or mutually visible.

For two premises, named A and B, the following weak inference deductions can be made:

- If A is true, B is false.
- If B is true, A is false.

Weak inference is represented in most notation systems by a dash sign: -.
Alternating Inference

In chains or loops, an interesting case occurs when the inference between subsequent pairs of premises alternates between strong and weak. Consider the following links:

- A weak link with B
- B strong link with C
- C weak link with D
- D strong link with E
- E weak link with F

Which can be written:

A - B = C - D = E - F

Where the links alternate between strong and weak, and the first and last link are the same, the chain is an Alternating Inference Chain (AIC). An AIC allows a link to be drawn directly between the two endpoints (see the AIC article for a proof of this). Thus, from the above chain we can derive:

A - F

and make any deductions that this weak link provides.

For this to work, the links must alternate strong and weak. You may be inclined to write the following chain:

A - B = C = D = E - F

However, this chain uses the wrong inference between premises C and D. This might be illustrated by showing the list of implications:

- If A is true, B is false.
- If B is false, C is true.
- If C is false, D is true.
- If D is false, E is true.
- If E is true, F is false.

There is no proper connection between the 3rd line and those preceding and succeeding it. If the strong link between C and D is one of those types of strong link that is also a weak link, we can convert it to a weak inference to write a correct chain:

A - B = C - D = E - F

Alternating inference guarantees that the logic is sound from the first to the last node in the chain.