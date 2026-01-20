public class SkeletonMonster : MonsterBase
{
    // Add skeleton-specific behavior here
    
    protected override void Start()
    {
        base.Start();
        // Custom initialization
    }
    
    protected override void MoveTowardsTarget()
    {
        // You can override movement or use the default
        base.MoveTowardsTarget();
        
        // Trigger your animation here
        // animator.SetBool("isWalking", true);
    }
}