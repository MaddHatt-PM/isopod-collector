# Isopod Collector

## Terms
### Biological Descriptions

### Gameplay elements
__Player__: Player

__Happiness Bursts__: A random event where an isopod can be tapped to trigger a small explosion of coins corresponding to the player's highest-attained revenue, the isopods current age, and the isopods current happiness value.  
A Happiness Burst is indicated by shiny particles on the chosen isopods.  
The isopods will not stop shining after a duration of time and will require the player to tap on them.  

__Stray Coins__: A random event where coins appear from isopods (dependent on happiness values).  
These coins will not go away but there is a limit to how many can be on screen at once.  

__Random Visitors__: A random event where any different species will come by with a gift.  
These gifts will offer higher and/or rarer rewards with the consideration that the insect's speed will affect how long the event lasts until the insect has left the screen.  

__Selling Off__: The player can sell off their isopods for a large sum of money.  
The amount of money an isopod is sold for will depend on its age and happiness value.  
If an isopod is particularly attached to a cosmetic, this will be factored in to account for the cost of the cosmetic.  

## Gameplay Loop
The player will be collecting, raising, and selling a variety of isopods by feeding them, interacting with them, and decorating their terrarium.  
In order to provide for them, the player will be generating passive interaction income by collecting __Happiness Bursts__, __Random Visitors__, and __Stray Coins__.  
Additionally, the player can actively gain income by __Selling Off__ an isopods for short-term gain.  

## Isopod Life Cycle
To convey a meaningful organism for the characters, Time-Inhomogeneous Markov Chains (inconsistent transition matrices) will be used over time. These transition matrices may likely be evaluated at offset time intervals to avoid consistent computational periods. This may incur a short duration of Gambler Ruin states. If computation load isn't too bad, the transition matrix may be revaluated per new state request.  

---

### Independent Variables
__Needs: Food Level__ $f'$  
Represents the ratio of the how full the isopod's stomach is with $f' = f'_\text{curr} / f'_{\max}$

__Needs: Water Level__ $w'$  
Represents the ratio of the how full the isopod's quench is with $w' = w'_\text{curr} / w'_{\max}$

__Needs: Social Battery__ $s'$  
Represents the ratio of the how much social battery the isopod has with $s' = s'_\text{curr} / s'_{\max}$

__Needs: Base Happiness__ $h'$  
Represents the ratio of the how happy the isopod has with $h' = h'_\text{curr} / h'_{\max}$

__Personality: Aging Multiplier__ $a'$  
Range $[-0.5, 0.5]$ for the threshold of fully grown.

__Rolling-Ability__ $r$  
Variable set per subspecies with 0 being never rolling and 1 being always rolling.  
This range will be remapped to an animation curve for remapping.  
Genetically does not increase/decrease consistently over time.


__Personality: Sociability__ $P_s$  
Range $[-0.5, +0.5]$ for no-social-happiness-bonus at $-0.5$ and max-social-happiness-bonus at $+0.5$.  
For overcrowding, the max-social-happiness bonus will become negated.  
This also affects the level of influence same-species isopod's will "mimic" each other.  

__Personality: Movement-Preference__ $P_m$  
Range $[0,1]$ for a tendency to move around or for resting.  
Genetically does not increase consistently over time.  

__Personality: Fun-Seeking__ $P_f$  
Range $[0,1]$ for a tendency to more likely seek out entertainment when needs are met.  

__Personality: Hunger-Multiplier__ $P_h$  
Range $[-0.5, +0.5]$ for how quickly an isopod will completely deplete its hunger.  

__Personality: Coin-Multiplier__ $P_c$  
Range $[0,1]$ for a multiplier of coins.  
Store-bought isopods will have a low level but future generations of isopods will consistently have higher coin-multiplier.  

__Personality: Repeating-Action__ $P_r$  
Range $[0,1]$ for how likely the isopod will repeat the previous state.

---

### Dependent Variables
Independent variables are listed in order of weakest to strongest weight.

__Age__ $a = \text{clamp01}( a_\text{curr} / (a_\text{max} * (a' * a'_\text{threshold})))$  
Current age $a_\text{curr}$ represents the number of game ticks that an isopod has lived.  
Threshold age $a_\text{max}$ represents the maximum number of game ticks that age can play a factor in decision making.  
The age multiplier threshold $a'_\text{threshold}$ restricts the degree of influence that age multiplier $a'$ can affect.  

__Food Satiation__ $f(P_m, a, P_h, f')$  
The degree of desire for food, normalized to $[0,1]$ space with $1$ being fully satiated with no desire.

__Water Satiation__ $w(P_m, a, w')$  
The degree of desire for water, normalized to $[0,1]$ space with $1$ being fully satiated with no desire.

__Social Satiation__ $s(a, P_m, s', P_s)$  
The degree of desire for socialization, normalized to $[0,1]$ space with $1$ being fully satiated with no desire.
Sociability is affected by the number of same-species and befriended isopods.  

__Rest Satiation__ $t(f', P_s, w', s', a, h', P_m)$  
The degree of desire for socialization, normalized to $[0,1]$ space with $1$ being fully satiated with no desire.

__Happiness Level__ $h(a, f', w', P_s, P_f,)$  

---

### Isopod States
We define $\lambda$ as a unique value per state to reduce repeating action.  

Subspecies restriction $\phi$ and global restriction $\Phi$ increases per arrival of the given State with a decreasing time-based cooldown.  
Note: This restrictions should not completely cancel out the probability for a state to occur.  

These states will be comprised of nodes to form a directed graph 

__Eating__ $\mathbb{N}_\text{E}(\lambda, P_r, P_s, f)$  
Closest food will be selected unless a favorite food is available.  

__Drinking__ $\mathbb{N}_\text{D}(\lambda, P_r, P_s, w)$  
Closest source of water is selected.

__Socializing__ $\mathbb{N}_\text{S}(\lambda, P_r, P_s)$  
An isopod will favor slightly favor their own subspecies but there is a percentage chance to befriend the closest isopod.

__Resting__ $\mathbb{N}_\text{R}(\lambda, P_r, P_s, t)$  
The isopod will either stop at its current position for rest or seek shelter with a user-placed item.  
Sleeping on a user-placed item will reward happiness points corresponding from the user-placed item.  

__Entertaining__ $\mathbb{E}(\lambda, f, w, h)$  
The isopod will seek out a place to have fun with a user-placed item.  

__Rewarding__ $\mathbb{R}(\lambda, h, \phi, \Phi)$  
The isopod will reward the player with an amount of gold from an interval generated from max-revenue and happiness level.  

__Special: Mating__ $\mathbb{S}_\text{M}(\lambda, f, w, \phi, a, s, \Phi)$  
Special state of Socialization that is attempted if a suitable mate is found.  
Suitable mate criteria involves: distance, subspecies compatibility, brooding-cooldown and social link.  
Failure to find a suitable mate will incur a small reduction in happiness $h$ and social battery $s'$.

__Special: Birthing__ $\mathbb{S}_\text{B}$  
Special state accessible at the end of the brooding period. 

__Special: Petting__ $\mathbb{S}_\text{P}$  
Special state triggered by interacting that has a chance for a reward based on happiness level $h$.  

__Special: Disappointment__ $\mathbb{S}_\text{D}$  
Special state triggered by reaching a state and there not being an opportunity to fulfill that state.  
