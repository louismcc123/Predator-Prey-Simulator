using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prey : MonoBehaviour
{
    public Vector3 targetPosition = new Vector3(0, 0, 0);
    //private Transform targetPosition;

    private NavMeshAgent agent;
    //private Transform target;

    public float speed = 0.2f;
    public float rotationSpeed = 6.0f;
    public float visionRadius = 10f;
    private float radius = 100;

    private float timeSinceLastUpdate = 0f;

    public GameObject preyPrefab;

    private bool foundInterest = false;
    private bool danger = false;

    public Species species;
    public Species diet;

    public float hunger;
    public float thirst;
    //public float stamina;

    float timeToDeathByHunger = 20;
    float timeToDeathByThirst = 20;

    public bool female;
    public float reproductiveUrge;
    float reproductiveTime = 100;
    float childCoolDown = 15;
    public float timeToNextChild;

    Environment environment;

    public enum PreyState
    {
        Wandering,
        Eat,
        Drink,
        Flee,
        Reproduce
    }

    [SerializeField] private PreyState preyState;

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

        preyState = PreyState.Wandering;
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

        switch (preyState)
        {
            case PreyState.Wandering:
                UpdateWandering();
                Wander(targetPosition);
                Debug.Log(gameObject.name + " is Wandering");
                break;

            case PreyState.Eat:
                UpdateEat();
                float distToPlant = Vector3.Distance(transform.position, FindClosestPlant().transform.position);

                if (distToPlant <= visionRadius)
                {
                    foundInterest = true;
                    targetPosition = FindClosestPlant().transform.position;

                    if (distToPlant <= 1)
                    {
                        hunger = 0;
                        foundInterest = false;
                    }
                }
                Debug.Log(gameObject.name + " is Hungry");
                break;

            case PreyState.Drink:
                UpdateDrink();

                float distToWater = Vector3.Distance(transform.position, FindClosestWater().transform.position);

                if (distToWater <= visionRadius)
                {
                    foundInterest = true;
                    targetPosition = FindClosestWater().transform.position;

                    if (distToWater <= 2)
                    {
                        thirst = 0;
                        foundInterest = false;
                    }
                }
                Debug.Log(gameObject.name + " is Thirsty");
                break;

            case PreyState.Reproduce:
                UpdateReproduce();
                Debug.Log(gameObject.name + " is Horny");
                break;

            case PreyState.Flee:
                UpdateFlee();
                Debug.Log(gameObject.name + " is Fleeing");
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
        if (!danger)
        {
            // HUNGRY
            if (hunger > 0.15f && hunger >= thirst && hunger >= reproductiveUrge)
            {
                preyState = PreyState.Eat;
            }

            // THIRSTY
            if (thirst > hunger && thirst >= reproductiveUrge)
            {
                preyState = PreyState.Drink;
            }

            //HORNY
            if (reproductiveUrge > hunger && reproductiveUrge > thirst && Time.time >= timeToNextChild)
            {
                preyState = PreyState.Reproduce;
            }
        }

        //FLEE
        else
        {
            preyState = PreyState.Flee;
        }
    }

    void UpdateFlee()
    {
        if (!danger)
        {
            preyState = PreyState.Wandering;
        }
    }

    void UpdateEat()
    {
        if (!danger)
        {
            // THIRSTY
            if (thirst > hunger && thirst >= reproductiveUrge)
            {
                preyState = PreyState.Drink;
            }

            // HORNY
            if (reproductiveUrge > hunger && reproductiveUrge > thirst && Time.time >= timeToNextChild)
            {
                preyState = PreyState.Reproduce;
            }

            // WANDER
            else if (!foundInterest)
            {
                targetPosition = environment.getRandomPosition(radius);
                Wander(targetPosition);
            }
        }

        //FLEE
        else
        {
            preyState = PreyState.Flee;
        }
    }

    void UpdateDrink()
    {
        if (!danger)
        {
            // HUNGRY
            if (hunger > 0.15f && hunger >= thirst && hunger >= reproductiveUrge)
            {
                preyState = PreyState.Eat;
            }

            // HORNY
            if (reproductiveUrge > hunger && reproductiveUrge > thirst && Time.time >= timeToNextChild)
            {
                preyState = PreyState.Reproduce;
            }

            // Move
            /*else
            {
                if(FindClosestWater() < visionRadius)
                {
                    foundInterest = true;
                    targetPosition = FindClosestWater();

                    float distToWater = Vector3.Distance(transform.position, targetPosition);

                    if (distToWater <= 1)
                    {
                        thirst = 0;
                        preyState = PreyState.Wandering;
                    }
                }
                else
                {
                    foundInterest= false;
                    targetPosition = environment.getRandomPosition(radius);
                    Wander(targetPosition);
                }
            }
        }
        //FLEE
        else
        {
            preyState = PreyState.Flee;*/
        }
    }

    void UpdateReproduce()
    {
        if (!danger)
        {
            // HUNGRY
            if (hunger >= thirst && hunger >= reproductiveUrge)
            {
                preyState = PreyState.Eat;
            }

            // THIRSTY
            if (thirst > hunger && thirst >= reproductiveUrge)
            {
                preyState = PreyState.Drink;
            }

            //WANDER
            if (!foundInterest)
            {
                targetPosition = environment.getRandomPosition(radius);
                Wander(targetPosition);
            }
        }

        // FLEE
        else
        {
            preyState = PreyState.Flee;
        }
    }

    void OnTriggerStay(Collider other)
    {
        /*if (other.CompareTag("Water") && preyState == PreyState.Drink)
        {
            foundInterest = true;

            targetPosition = other.transform.position;

            float dist = Vector3.Distance(transform.position, targetPosition);

            if (dist <= 1)
            {
                thirst = 0;
                foundInterest = false;
            }
        }*/
        if (other.TryGetComponent<LiveEntity>(out LiveEntity entity))
        {
            // Flee
            if (entity.species == Species.Predator)
            {
                danger = true;

                Vector3 dirToDanger = transform.position - other.transform.position;
                targetPosition = transform.position + dirToDanger;

                float dist = Vector3.Distance(transform.position, targetPosition);

                if (dist > visionRadius)
                {
                    danger = false;
                }

                if (dist <= 1)
                {
                    GetComponent<LiveEntity>().Die(CauseOfDeath.beingEaten);
                }
            }

            // Reproduce
            if (entity.species == species && preyState == PreyState.Reproduce)
            {
                foundInterest = true;

                Prey otherPrey = entity.GetComponent<Prey>();

                if (otherPrey.preyState == Prey.PreyState.Reproduce)
                {
                    if (female && otherPrey.female == false || !female && otherPrey.female == true)
                    {
                        targetPosition = otherPrey.transform.position;

                        float dist = Vector3.Distance(transform.position, targetPosition);

                        if (dist <= 1)
                        {
                            if(female) 
                            { 
                                timeToNextChild = Time.time + childCoolDown;

                                var preyChild = Instantiate(preyPrefab, transform.position,
                                    Quaternion.identity).GetComponent<Prey>();

                                preyChild.hunger = 0;
                                preyChild.thirst = 0;
                                preyChild.reproductiveUrge = 0;
                                preyChild.foundInterest = false;
                                preyChild.preyState = Prey.PreyState.Wandering;
                                preyChild.female = Random.value < 0.5f;

                                // mutations:

                                if (Random.value < 0.05f)
                                {
                                    preyChild.visionRadius = visionRadius + Random.Range(-1f, 1f);
                                }
                                else
                                {
                                    if (Random.value < 0.475f)
                                    {
                                        preyChild.visionRadius = visionRadius;
                                    }
                                    else
                                    {
                                        preyChild.visionRadius = otherPrey.visionRadius;
                                    }
                                }

                                if (Random.value < 0.05f)
                                {
                                    preyChild.speed = speed + Random.Range(-1f, 1f);
                                    preyChild.rotationSpeed = rotationSpeed + Random.Range(-1f, 1f);
                                }
                                else
                                {
                                    if (Random.value < 0.475f)
                                    {
                                        preyChild.speed = speed;
                                        preyChild.rotationSpeed = rotationSpeed;
                                    }
                                    else
                                    {
                                        preyChild.speed = otherPrey.speed;
                                        preyChild.rotationSpeed = otherPrey.rotationSpeed;
                                    }
                                }

                                if (Random.value < 0.05f)
                                {
                                    preyChild.timeToDeathByHunger = timeToDeathByHunger + Random.Range(-1f, 1f);
                                }
                                else
                                {
                                    if (Random.value < 0.475f)
                                    {
                                        preyChild.timeToDeathByHunger = timeToDeathByHunger;
                                    }
                                    else
                                    {
                                        preyChild.timeToDeathByHunger = otherPrey.timeToDeathByHunger;
                                    }
                                }

                                if (Random.value < 0.05f)
                                {
                                    preyChild.timeToDeathByThirst = timeToDeathByThirst + Random.Range(-1f, 1f);
                                }
                                else
                                {
                                    if (Random.value < 0.475f)
                                    {
                                        preyChild.timeToDeathByThirst = timeToDeathByThirst;
                                    }
                                    else
                                    {
                                        preyChild.timeToDeathByThirst = otherPrey.timeToDeathByThirst;
                                    }
                                }

                                if (Random.value < 0.05f)
                                {
                                    preyChild.reproductiveTime = reproductiveTime + Random.Range(-1f, 1f);
                                }
                                else
                                {
                                    if (Random.value < 0.475f)
                                    {
                                        preyChild.reproductiveTime = reproductiveTime;
                                    }
                                    else
                                    {
                                        preyChild.reproductiveTime = otherPrey.reproductiveTime;
                                    }
                                }
                            }

                            foundInterest = false;
                            reproductiveUrge = 0;
                        }
                    }
                }
            }
            // Eat
            /*if (entity.species == diet) 
            {
                float dist = Vector3.Distance(transform.position, entity.transform.position);

                if (preyState == PreyState.Eat)
                {
                    foundInterest = true;
                    targetPosition = entity.transform.position;

                    if (dist <= 1)
                    {
                        entity.GetComponent<LiveEntity>().Die(CauseOfDeath.beingEaten);

                        hunger = 0;
                        foundInterest = false;
                    }
                }
                else
                {
                    if (dist <= 1)
                    {
                        entity.GetComponent<LiveEntity>().Die(CauseOfDeath.beingTrampled);
                    }
                }
            }
        }
        else
        {
            if (foundInterest == true)
            {
                foundInterest = false;
            }
        }*/
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

    public GameObject FindClosestPlant()
    {
        GameObject[] plants;
        plants = GameObject.FindGameObjectsWithTag("Plant");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject plant in plants)
        {
            Vector3 diff = plant.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = plant;
                distance = curDistance;
            }
        }
        return closest;
    }

    public void OnMouseDown()
    {
        //Need to sort out UI control 
        
        CameraController.instance.followTransform = transform;
    } 
}