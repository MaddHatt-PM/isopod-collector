using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.Remoting.Messaging;

public class IsopodStatus : MonoBehaviour
{
  [Header("Age")]
  [SerializeField, ReadOnly] private long birthTime;
  [SerializeField, ReadOnly, Range(0f, 1f)] private float lifeStage;
  [SerializeField, ReadOnly] private float age;

  [Header("External Need Levels")]
  [InfoBox("These attributes change over time based on biological needs and range from [0,1].")]
  [SerializeField] private float currentFoodLevel;
  [SerializeField] private float currentWaterLevel;
  [SerializeField] private float currentSocialLevel;
  [SerializeField] private float currentHappinessLevel;

  [SerializeField, Range(0f, 1f)] private float foodLevel;
  [SerializeField, Range(0f, 1f)] private float waterLevel;
  [SerializeField, Range(0f, 1f)] private float socialLevel;
  [SerializeField, Range(0f, 1f)] private float happinessLevel;

  [Header("Personality Traits")]
  [InfoBox("These attributes are determined at birth and range from [-0.5, 0.5].")]
  [SerializeField, Range(-0.5f, 0.5f)] private float agingness;
  [SerializeField, Range(-0.5f, 0.5f)] private float rollingness;
  [SerializeField, Range(-0.5f, 0.5f)] private float socialness;
  [SerializeField, Range(-0.5f, 0.5f)] private float lethargicness;
  [SerializeField, Range(-0.5f, 0.5f)] private float funSeekingness;
  [SerializeField, Range(-0.5f, 0.5f)] private float hungryness;
  [SerializeField, Range(-0.5f, 0.5f)] private float coinBonusness;
  [SerializeField, Range(-0.5f, 0.5f)] private float repetiveness;

  // [Header("Life Stage Traits")]
}
