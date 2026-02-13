using System.Collections;
using System.Collections.Generic;
using Game.Core;
using Game.Feature.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Feature.Behaviors
{
    public class NPCBrain : MonoBehaviour
    {
        private static readonly int PickUp = Animator.StringToHash("PickUp");
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int Drop = Animator.StringToHash("Drop");

        //todo:Inject
        [Header("System References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private Transform golfCart;
        
        [Header("References")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform itemHolder;
    
        [Header("Settings")]
        [SerializeField] private float collectionDistance = 1.5f;
    
        private ICollectionStrategy currentStrategy;
        private GolfBall targetBall;
        private GolfBall currentBall;
        private List<ICollectable> availableTargets = new();
    
        private enum State { SearchingForBall, MovingToBall, ReturningToCart }
        private State currentState = State.SearchingForBall;
    
        private void Start()
        {
            currentStrategy = new ClosestStrategy();
            
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            
            agent.stoppingDistance = collectionDistance; 

            FindAllBalls();
        }
    
        private void Update()
        {
            if (!healthSystem.IsAlive) 
            {
                if (agent.enabled) agent.isStopped = true;
                animator.SetBool(IsWalking,false);
                animator.ResetTrigger(Drop);
                animator.ResetTrigger(PickUp);
                return;
            }
        
            switch (currentState)
            {
                case State.SearchingForBall:
                    SearchForTargetBall();
                    break;
                case State.MovingToBall:
                    MoveTowardsBall();
                    break;
                case State.ReturningToCart:
                    ReturnToCart();
                    break;
            }
        }
    
        private void FindAllBalls()
        {
            availableTargets.Clear();
            availableTargets.AddRange(FindObjectsOfType<GolfBall>());
        }
    
        private void SearchForTargetBall()
        {
            if (availableTargets.Count == 0) FindAllBalls();

            targetBall = currentStrategy.SelectTarget(availableTargets, transform.position, healthSystem.CurrentHealth) as GolfBall;
        
            if (targetBall != null)
            {
                currentState = State.MovingToBall;
                agent.SetDestination(targetBall.transform.position);
            }
        }

        private void MoveTowardsBall()
        {
            if (currentBall!=null)
            {
                return;
            }
            if (targetBall == null)
            {
                currentState = State.SearchingForBall;
                return;
            }

            animator.SetBool(IsWalking,true);
            agent.SetDestination(targetBall.transform.position);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;
                StartCoroutine(CollectBall(targetBall));
            }
        }

        private IEnumerator CollectBall(GolfBall ball)
        {
            currentBall = ball;
            availableTargets.Remove(ball);
            targetBall = null;
            
            animator.SetBool(IsWalking,false);
            animator.SetTrigger(PickUp);
            
            yield return new WaitForSeconds(.5f);
            
            ball.Collect(itemHolder);
            
            yield return new WaitForSeconds(1f);

            
            currentState = State.ReturningToCart;
            if (golfCart != null)
            {
                agent.SetDestination(golfCart.position);
                agent.isStopped = false;
            }
        }
    
        private void ReturnToCart()
        {
            if (currentBall==null)
            {
                return;
            }
            if (golfCart == null) 
            {
                currentState = State.SearchingForBall;
                return;
            }

            animator.SetBool(IsWalking,true);
            agent.SetDestination(golfCart.position);

            if (!agent.pathPending && agent.remainingDistance <= 3f)
            {
                agent.isStopped = true;
                agent.ResetPath();
                
                if (currentBall!=null)
                {
                    animator.SetTrigger(Drop);
                    scoreSystem.AddScore(currentBall.PointValue);
                }
                StartCoroutine(DropBallAndSearch(currentBall));
                currentBall = null;
            }
        }

        private IEnumerator DropBallAndSearch(GolfBall ball)
        {
            yield return new WaitForSeconds(.55f); 

            ball.Drop();
            agent.isStopped = false;
            yield return new WaitForSeconds(.4f); 

            currentState = State.SearchingForBall;
        }
    }
}