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

[RequireComponent(typeof(PlayerCombatRanged), typeof(PlayerCombatMelee))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerParrybox parrybox;
    [SerializeField] private LayerMask targetLayer;

    // Combat Components
    private PlayerCombatRanged _rangedAttack;
    private PlayerCombatMelee _meleeAttack;

    private bool _combatInputEnabled;

    // Requested Inputs
    private bool _requestedRanged;
    private bool _requestedMelee;
    private bool _requestedParry;
    private Vector3 _requestedMousePosition;

    // State Machine
    private CombatState _state;
    private CombatState _prevState;

    // Action trackers
    private bool _meleeStarted;
    private bool _parryStarted;

    // Start()
    public void Initialize(PlayerAnimationController animationController, CapsuleCollider hurtbox)
    {
        _rangedAttack = GetComponent<PlayerCombatRanged>();
        _meleeAttack = GetComponent<PlayerCombatMelee>();

        _rangedAttack.Initialize(targetLayer);
        _meleeAttack.Initialize(targetLayer);
        parrybox.Initialize(animationController, hurtbox);

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
            _requestedParry = (input.Parry && _state.CurrentAction is not CombatAction.Melee) || _requestedParry;

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
            _requestedRanged = _requestedMelee = false;

            _requestedParry = _parryStarted = false;
            parrybox.ParryboxEnabled(false);
        }
    }

    // LateUpdate()
    public void UpdateCombatAction(float deltaTime)
    {
        _state.Target = _requestedMousePosition;

        HandleParryRequest(deltaTime);

        // State Machine Control
        switch (_state.CurrentAction)
        {
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
    // PARRY
    private void HandleParryRequest(float deltaTime)
    {
        if (_requestedParry && !_parryStarted)
        {
            _requestedParry = false;
            if (parrybox.CanParry())
            {
                _parryStarted = true;

                parrybox.ParryboxEnabled(true);
            }
        }
        parrybox.UpdateParrybox(ref _parryStarted, deltaTime);
    }
    // MELEE
    private void OnMeleeAttack(float deltaTime)
    {
        _meleeAttack.Attack(ref _state, ref _meleeStarted, ref _combatInputEnabled, deltaTime);

        // Melee Combo START
        if (!_meleeStarted && _state.CurrentAction is CombatAction.Melee)
        {
            _meleeStarted = true;
            Player.Instance.MovementInputEnabled(false);
            _meleeAttack.TriggerAttack();
        }

        // Melee Combo CONTINUE
        if (_requestedMelee)
        {
            _meleeAttack.TriggerAttack();
        }
    }
    // RANGED
    private void OnRangedAttack(float deltaTime)
    {
        // Trigger Attack
        _rangedAttack.Attack(ref _state, deltaTime);

        // Update Combat State
        _state.CurrentAction = !_requestedRanged ? CombatAction.None : _state.CurrentAction;
    }
    #endregion


    #region *--- Helper Functions -------------------------------------*
    private void TryEnterNewState()
    {
        _state.CurrentAction = _requestedMelee ? CombatAction.Melee : _requestedRanged ? CombatAction.Ranged : _state.CurrentAction;
    }
    #endregion


    #region *--- Public Getters/Setters ----------------------------------------*
    // State Getters
    public CombatState GetState() => _state;
    public CombatState GetPrevState() => _prevState;

    // CombatAction Setters
    public void SetCurrentCombatAction(CombatAction action) => _state.CurrentAction = action;
    #endregion


    


    #region *--- Public Methods to Influence Player Combat Actions ------------------------------*
    public void CombatInputEnabled(bool b)
    {
        if (!b)
            _combatInputEnabled = _requestedMelee = _requestedRanged = _requestedParry = false;
        else 
            _combatInputEnabled = b;
    }
    public void MeleeHitboxEnabled(bool b) => _meleeAttack.HitboxEnabled(b);
    /// * Enable/Disable ATTACK inputs
    /// * Enable/Disable PARRY inputs
    /// * Enable/Disable ALL COMBAT inputs
    #endregion
}