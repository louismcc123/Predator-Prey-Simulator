using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Predator : MonoBehaviour
{
    public Vector3 targetPosition = new Vector3(0, 0, 0);

    private NavMeshAgent agent;

    public float speed = 5.0f;
    public float rotationSpeed = 8.0f;
    public float visionRadius = 15f;
    private float radius = 100;

    private float timeSinceLastUpdate = 0f;

    public GameObject predatorPrefab;

    private bool foundInterest = false;

    public Species species;
    public Species diet;

    public float hunger;
    public float thirst;

    float timeToDeathByHunger = 20;
    float timeToDeathByThirst = 20;

    public bool female;
    public float reproductiveUrge;
    float reproductiveTime = 50;
    float childCoolDown = 10;
    public float timeToNextChild;

    Environment environment;

    public enum PredatorState
    {
        Wandering,
        Eat,
        Drink,
        Reproduce
    }

    [SerializeField] private PredatorState predatorState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        environment = FindObjectOfType<Environment>();

        female = Random.value < 0.5f;

        //targetPosition = environment.getRandomPosition(radius);

        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();

        trigger.radius = visionRadius;
        trigger.isTrigger = true;

        predatorState = PredatorState.Wandering;
    }

    void Update()
    {
        float hungerTime = speed / 10;
        float thirstTime = speed / 10;

        hunger += Time.deltaTime * hungerTime / timeToDeathByHunger;
        thirst += Time.deltaTime * thirstTime / timeToDeathByThirst;
        reproductiveUrge += Time.deltaTime / reproductiveTime;
        timeSinceLastUpdate += Time.deltaTime;


        if (hunger >= 1)
        {
            GetComponent<LiveEntity>().Die(CauseOfDeath.hunger);
        }

        if (thirst >= 1)
        {
            GetComponent<LiveEntity>().Die(CauseOfDeath.thirst);
        }

        switch (predatorState)
        {
            case PredatorState.Wandering:
                UpdateWandering();
                Wander(targetPosition);
                break;

            case PredatorState.Eat:
                UpdateEat();
                break;

            case PredatorState.Drink:
                UpdateDrink();

                float dist = Vector3.Distance(transform.position, FindClosestWater().transform.position);

                if (dist <= visionRadius)
                {
                    foundInterest = true;
                    targetPosition = FindClosestWater().transform.position;

                    if (dist <= 2)
                    {
                        thirst = 0;
                        foundInterest = false;
                    }
                }
                break;

            case PredatorState.Reproduce:
                UpdateReproduce();
                break;
        }

        Move(targetPosition);
    }

    void Move(Vector3 targetPosition)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(targetPosition - transform.position),
            rotationSpeed * Time.deltaTime);

        transform.position += transform.forward * speed * Time.deltaTime;

        agent.destination = targetPosition;
    }

    void Wander(Vector3 targetPosition)
    {
        if (timeSinceLastUpdate > 1f)
        {
            targetPosition = environment.getRandomPosition(radius);

            timeSinceLastUpdate = 0f;
        }
    }

    void UpdateWandering()
    {
        // HUNGRY
        if (hunger > 0.15f && hunger >= thirst && hunger >= reproductiveUrge)
        {
            predatorState = PredatorState.Eat;
        }

        // THIRSTY
        if (thirst > hunger && thirst >= reproductiveUrge)
        {
            predatorState = PredatorState.Drink;
        }

        //HORNY
        if (reproductiveUrge > hunger && reproductiveUrge > thirst && Time.time >= timeToNextChild)
        {
            predatorState = PredatorState.Reproduce;

        }
    }

    void UpdateEat()
    {
        // THIRSTY
        if (thirst > hunger && thirst > reproductiveUrge)
        {
            predatorState = PredatorState.Drink;
        }

        // HORNY
        if (reproductiveUrge > hunger && reproductiveUrge > thirst && Time.time >= timeToNextChild)
        {
            predatorState = PredatorState.Reproduce;
        }

        // WANDER
        else if (!foundInterest)
        {
            targetPosition = environment.getRandomPosition(radius);
            Wander(targetPosition);
        }
    }

    void UpdateDrink()
    {
        // HUNGRY
        if (hunger > 0.15f && hunger >= thirst && hunger >= reproductiveUrge)
        {
            predatorState = PredatorState.Eat;
        }

        // HORNY
        if (reproductiveUrge > hunger && reproductiveUrge > thirst && Time.time >= timeToNextChild)
        {
            predatorState = PredatorState.Reproduce;
        }

        // WANDER
        else if (!foundInterest)
        {
            targetPosition = environment.getRandomPosition(radius);
            Wander(targetPosition);
        }
    }

    void UpdateReproduce()
    {
        // HUNGRY
        if (hunger > 0.15f && hunger >= thirst && hunger >= reproductiveUrge)
        {
            predatorState = PredatorState.Eat;
        }

        // THIRSTY
        if (thirst > hunger && thirst > reproductiveUrge)
        {
            predatorState = PredatorState.Drink;
        }

        //WANDER
        if (!foundInterest)
        {
            targetPosition = environment.getRandomPosition(radius);
            Wander(targetPosition);
        }
    }

    void OnTriggerStay(Collider other)
    {
        /*if (other.CompareTag("Water") && predatorState == PredatorState.Drink)
        {
            foundInterest = true;

            targetPosition = other.transform.position;

            float dist = Vector3.Distance(transform.position, new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z));

            if (dist <= 2)
            {
                thirst = 0;
                foundInterest = false;
            }
        }*/
        if (other.TryGetComponent<LiveEntity>(out LiveEntity entity))
        {
            // Reproduce
            if (entity.species == species && predatorState == PredatorState.Reproduce)
            {
                foundInterest = true;

                Predator otherPredator = entity.GetComponent<Predator>();

                if (otherPredator.predatorState == Predator.PredatorState.Reproduce)
                {
                    if (female && otherPredator.female == false || !female && otherPredator.female == true)
                    {
                        targetPosition = otherPredator.transform.position;

                        float dist = Vector3.Distance(transform.position, targetPosition);

                        if (dist <= 2 && female)
                        {
                            timeToNextChild = Time.time + childCoolDown;

                            var predatorChild = Instantiate(predatorPrefab, transform.position,
                                Quaternion.identity).GetComponent<Predator>();

                            predatorChild.hunger = 0;
                            predatorChild.thirst = 0;
                            predatorChild.reproductiveUrge = 0;
                            predatorChild.foundInterest = false;
                            predatorChild.predatorState = Predator.PredatorState.Wandering;
                            predatorChild.female = Random.value < 0.5f;

                            // mutations:

                            if (Random.value < 0.05f)
                            {
                                predatorChild.visionRadius = visionRadius + Random.Range(-1f, 1f);
                            }
                            else
                            {
                                if (Random.value < 0.475f)
                                {
                                    predatorChild.visionRadius = visionRadius;
                                }
                                else
                                {
                                    predatorChild.visionRadius = otherPredator.visionRadius;
                                }
                            }

                            if (Random.value < 0.05f)
                            {
                                predatorChild.speed = speed + Random.Range(-1f, 1f);
                                predatorChild.rotationSpeed = rotationSpeed + Random.Range(-1f, 1f);
                            }
                            else
                            {
                                if (Random.value < 0.475f)
                                {
                                    predatorChild.speed = speed;
                                    predatorChild.rotationSpeed = rotationSpeed;
                                }
                                else
                                {
                                    predatorChild.speed = otherPredator.speed;
                                    predatorChild.rotationSpeed = otherPredator.rotationSpeed;
                                }
                            }

                            if (Random.value < 0.05f)
                            {
                                predatorChild.timeToDeathByHunger = timeToDeathByHunger + Random.Range(-1f, 1f);
                            }
                            else
                            {
                                if (Random.value < 0.475f)
                                {
                                    predatorChild.timeToDeathByHunger = timeToDeathByHunger;
                                }
                                else
                                {
                                    predatorChild.timeToDeathByHunger = otherPredator.timeToDeathByHunger;
                                }
                            }

                            if (Random.value < 0.05f)
                            {
                                predatorChild.timeToDeathByThirst = timeToDeathByThirst + Random.Range(-1f, 1f);
                            }
                            else
                            {
                                if (Random.value < 0.475f)
                                {
                                    predatorChild.timeToDeathByThirst = timeToDeathByThirst;
                                }
                                else
                                {
                                    predatorChild.timeToDeathByThirst = otherPredator.timeToDeathByThirst;
                                }
                            }

                            if (Random.value < 0.05f)
                            {
                                predatorChild.reproductiveTime = reproductiveTime + Random.Range(-1f, 1f);
                            }
                            else
                            {
                                if (Random.value < 0.475f)
                                {
                                    predatorChild.reproductiveTime = reproductiveTime;
                                }
                                else
                                {
                                    predatorChild.reproductiveTime = otherPredator.reproductiveTime;
                                }
                            }

                            foundInterest = false;
                            reproductiveUrge = 0;
                        }
                    }
                }
            }
            // Eat
            if (entity.species == diet && predatorState == PredatorState.Eat)
            {
                foundInterest = true;
                targetPosition = entity.transform.position;

                float dist = Vector3.Distance(transform.position, targetPosition);

                if (dist <= 2)
                {
                    entity.GetComponent<LiveEntity>().Die(CauseOfDeath.beingEaten);
                    hunger = 0;
                    foundInterest = false;
                }
            }
        }
        else
        {
            if (foundInterest == true)
            {
                foundInterest = false;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Plant") 
        {
            collision.gameObject.GetComponent<LiveEntity>().Die(CauseOfDeath.beingTrampled);
        }
    }

    public GameObject FindClosestWater()
    {
        GameObject[] water;
        water = GameObject.FindGameObjectsWithTag("Water");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject go in water)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

        public void OnMouseDown()
    {
        CameraController.instance.followTransform = transform;
    } // camera
}