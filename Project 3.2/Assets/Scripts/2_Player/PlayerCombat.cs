using UnityEngine;

public struct CombatState
{
    public CombatAction CurrentAction;
    public Vector3 Target;
}
public enum CombatAction
{
    None    = 0,
    Ranged  = 1,
    Melee   = 2,
    Parry   = 3
}
public struct CombatInput
{
    public bool Ranged;
    public bool Melee;
    public bool Parry;
    public Vector3 MousePosition;
}

public class PlayerCombat : MonoBehaviour
{
    public bool ShowDebug;

    [Header("Player Attacks")]
    [SerializeField] private RangedAttack rangedAttack;
    [SerializeField] private MeleeAttack meleeAttack;

    [Header("Ranged Attack Components")]
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform projectileSpawn;

    [Header("Melee Attack Components")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private Transform meleeHitbox;
    [SerializeField] private float meleeHitboxRadius = 1f;
    private bool _meleeStarted;

    // Requested Inputs
    private bool _requestedRanged;
    private bool _requestedMelee;
    private bool _requestedParry;
    private Vector3 _requestedMousePosition;
    private bool _combatInputEnabled;

    // State Machine
    private CombatState _state;
    private CombatState _prevState;

    void OnDrawGizmos()
    {
        if (!ShowDebug) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleeHitbox.position, meleeHitboxRadius);
    }

    // Start()
    public void Initialize()
    {
        rangedAttack.Initialize(projectilePool, projectileSpawn);
        meleeAttack.Initialize(animationController, meleeHitbox, meleeHitboxRadius);

        _combatInputEnabled = true;

        _state.CurrentAction = CombatAction.None;
        _state.Target = Vector3.forward;
        _prevState = _state;
    }

    // Update()
    public void UpdateInput(CombatInput input)
    {
        if (_combatInputEnabled && Player.Instance.GetCurrentMovementAction() is not MovementAction.Roll)
        {
            // Parry should only be available if the button is pressed
            //  AND we're not performing a melee attack
            _requestedParry = input.Parry && _state.CurrentAction is not CombatAction.Melee;

            // Melee attack should only be available if the button is pressed
            //  AND we're not performing a parry
            _requestedMelee = input.Melee && _state.CurrentAction is not CombatAction.Parry;

            // Ranged attack should only be available if the button is pressed
            //  AND we're not performing a melee attack
            //  AND we're not performing a parry
            _requestedRanged = input.Ranged && _state.CurrentAction is not CombatAction.Melee or CombatAction.Parry;

            _requestedMousePosition = input.MousePosition;
        }
        else
        {
            _requestedRanged = _requestedMelee = _requestedParry = false;
        }
    }

    // LateUpdate()
    public void UpdateCombatAction(float deltaTime)
    {
        _state.Target = _requestedMousePosition;

        // State Machine Control
        switch (_state.CurrentAction)
        {
            case CombatAction.Parry:
                OnParry();
                break;
            case CombatAction.Melee:
                OnMeleeAttack(deltaTime);
                break;
            case CombatAction.Ranged:
                OnRangedAttack(deltaTime);
                break;
            default:
                TryEnterNewState();
                break;
        };  

        _prevState = _state;
    }

    #region *--- Combat Action Functions ------------------------------*
    private void OnParry()
    {
        
    }
    private void OnMeleeAttack(float deltaTime)
    {
        meleeAttack.Attack(ref _state, ref _meleeStarted, ref _combatInputEnabled, deltaTime);

        // Melee Combo START
        if (!_meleeStarted && _state.CurrentAction is CombatAction.Melee)
        {
            _meleeStarted = true;
            Player.Instance.MovementInputEnabled(false);
            meleeAttack.TriggerAttack();
        }

        // Melee Combo CONTINUE
        if (_requestedMelee)
        {
            meleeAttack.TriggerAttack();
        }
    }
    private void OnRangedAttack(float deltaTime)
    {
        // Trigger Attack
        rangedAttack.Attack(ref _state, deltaTime);

        // Update Combat State
        _state.CurrentAction = !_requestedRanged ? CombatAction.None : _state.CurrentAction;
    }
    #endregion


    #region *--- Helper Functions -------------------------------------*
    private void TryEnterNewState()
    {
        _state.CurrentAction = _requestedParry ? CombatAction.Parry
                                : _requestedMelee ? CombatAction.Melee
                                    : _requestedRanged ? CombatAction.Ranged : CombatAction.None;
    }
    #endregion


    #region *--- Public Getters ----------------------------------------*
    public CombatState GetState() => _state;
    public CombatState GetPrevState() => _prevState;
    #endregion


    #region *--- Public Methods to Influence Player Combat Actions ------------------------------*
    public void CombatInputEnabled(bool b)
    {
        if (!b)
            _combatInputEnabled = _requestedMelee = _requestedRanged = _requestedParry = false;
        else 
            _combatInputEnabled = b;
    }
    public void MeleeHitboxEnabled(bool b) => meleeAttack.HitboxEnabled(b);
    /// * Enable/Disable ATTACK inputs
    /// * Enable/Disable PARRY inputs
    /// * Enable/Disable ALL COMBAT inputs
    #endregion
}