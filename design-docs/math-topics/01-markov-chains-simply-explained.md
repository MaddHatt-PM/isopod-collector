# [Markov Chains Simply Explained](https://towardsdatascience.com/markov-chains-simply-explained-dc77836b47e3)
__An intuitive and simple explanation of the Markov Property and Markov Chain__  
Markov Chains appear in many fields, including Physics, Genetics, Finance, Data Science and Computer Science.  

___Def: Markov Property___: The probability of the next state only depends on the current state.  
$\quad$ Note: Everything prior to the current state is irrelevant.  
$\quad$ Mathematically, this is written as probability function $P$ such that,
$$
P\Big(X_n = s_n \space\Big|\space X_{n-1} = s_{n-1}, \dots, X_0 = s_0\Big) = P\Big(X_n = s_n \space\Big|\space X_{n-1} = s_{n-1}\Big)
\\[1em]
\qquad\text{where}\qquad
\begin{aligned}
n &: \text{Time step parameter,} \\
X &: \text{Random variable that takes on a value in a given state space $s$} \\
s &: \text{Set of probabilities with corresponding states} \\
\end{aligned}
$$

___Def: Markov Process___: A process that uses the Markov Property.  
___Def: Markov Chain___: A sequence of Markov Processes given a finite state space $s$ with discrete time-steps $n$.  
This article will describe ___time-homogenous discrete-time Markov Chains___ where the transition probability between state is fixed between transitions.  

> __Isopod Game Note__  
> Isopod Game will need to use ___time-inhomogeneous Markov Chains___ to consider other variables.  
> This topic might be difficult to research as they become difficult to analyze.  
> Additional Resources:  
> This post from [MathOverflow](https://mathoverflow.net/questions/168398/time-inhomogeneous-markov-chains).  
> This paper on [Merge Times and Hitting Times of Time-Inhomogeneous Markov Chains](https://dukespace.lib.duke.edu/dspace/bitstream/handle/10161/8918/Jiarou%20Shen_Math%20Thesis.pdf?sequence=1#:~:text=A%20Markov%20chain%20is%20a,ability%20matrices%20at%20each%20step.).  

A useful representation of state changes will be to treat the collection of weights as an edge adjacency matrix to create a directed graph.  
This edge adjacency matrix is also known as a ___probability transition matrix___.  
We define a transition matrix $P$ with $i$ rows and $j$ columns such that the probability of the transition $x,y$ is at $P_{xy}$.

> __Isopod Game Note__  
> As Isopod Game will utilize different parameters to compute weight, it may be useful to create a tool to create directed graphs with pre-computed weights based on configurable parameter sliders.  

