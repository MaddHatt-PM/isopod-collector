# [Markov Chains: Multi-Step Transitions](https://towardsdatascience.com/markov-chains-multi-step-transitions-6772114bcc1d)
__Understanding multi-step transitions in Markov Chains using Chapman-Kolmogorov Equations__  

Recall that a sampling of a transition matrix in a Markov Chain is defined as a ___one-step transition___.  
Consider a set of states $s = \{ A, B, C\}$.  
We define a notation of, 
$$
\LARGE
P^n_{a,b}
\normalsize

\qquad\text{where}\qquad
\begin{aligned}
n &: \text{Number of transitions time steps,} \\
a &: \text{Initial state,} \\
b &: \text{Destination state.} \\
\end{aligned}
$$

For example purposes, let $P_{ij}$ as,
$$
P_ij =
\begin{bmatrix}
    0   & 0.4 & 0.6 \\
    0.5 & 0.3 & 0.2 \\
    0.5 & 0.3 & 0.2 \\
\end{bmatrix}
$$
We can describe a particular two step transition between $B$ to $A$ as,
$$
\begin{aligned}
P^2_{B,A} &= (0.5 \times 0.3) + (0.2 \times 0.5) \\
&= 0.25.
\end{aligned}
$$
However, this approach becomes increasingly difficult when the state space grows larger and we need more transitions.  

We can generalize a multi-step transition with the notation,
$$
\Large
P_{i,j}^{m,n} = P\Big(X_n = j \space\Big|\space X_m = i\Big)
\normalsize

\qquad\text{where}\qquad
\begin{aligned}
m &: \text{Initial time step,} \\
n &: \text{Final time step,} \\
i &: \text{Initial state,} \\
j &: \text{Final state.} \\
\end{aligned}
$$

We define $l\in(m,n)$ and $k$ as an indexing variable in the state space.  
We now define the following equation as the Chapman-Kolmogorov Equation,
$$
\Large
P_{i,j}^{m,n} = \sum_k P_{i,k}^{m,l}P_{k,j}^{l,n}
$$

Returning to our example, we describe the probability of moving from state B to state A with 2 timesteps as,
$$
\begin{aligned}
P_{B,A}^{0,2} 
&= \sum_k P_{B,k}^{0,1} P_{k,A}^{1,2} \\[1em]
&= P_{B,A}^{0,1} P_{A,A}^{1,2} + P_{B,B}^{0,1} P_{B,A}^{1,2} + P_{B,C}^{0,1} P_{C,A}^{1,2} \\[1em]
&= (0.3 \times 0) + (0.3 \times 0.5) + (0.2 \times 0.5) \\[1em]
&= 0.25.
\end{aligned}
$$
Intuitively, this equation is breaking down the two-step transition into a single one-step duration.

Although the initial timestep was set at 0, we can start our equation at any given state as Markov Chains are stateless.  

Now consider a timestep duration of 3.  
As $l$ could be either 1 or 2, we would need to apply the Chapman Kolmogorov equation twice recursively to express the formula as a one-step transition.  

## Multi-Step Transition Matrices
Recall that transition matrices are square matrices.  
As they are square, we can take a power $n$ of the matrices by matrix multiplying it by itself $n$ times.  
By doing this operation, we will also arrive at the probability of all states for an $n$-timestep-duration transition.  

Recall that powers of matrices are able to be calculated easier with diagonalization, provided that the transformation matrix $C$ and its inverse $C^{-1}$ exist.