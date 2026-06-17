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
    #region * Variables --------------------------------------------------
    [SerializeField] private PlayerParrybox parrybox;
    [SerializeField] private LayerMask targetLayer;

    // Combat Components
    private PlayerCombatRanged _rangedAttack;
    private PlayerCombatMelee _meleeAttack;

    // Input Status
    private bool _combatInputEnabled;
    private bool _parryInputEnabled;

    // Requested Inputs
    private bool _requestedRanged;
    private bool _requestedMelee;
    private bool _requestedParry;
    private Vector3 _requestedMousePosition;

    // State Machine
    private CombatState _state;
    private CombatState _prevState;

    // Action trackers
    private bool _parryStarted;
    #endregion

    #region * Initialization --------------------------------------------------
    // Called by 'Player.cs' in 'Start()' function
    public void Initialize(PlayerAnimationController animationController, CapsuleCollider hurtbox)
    {
        _rangedAttack = GetComponent<PlayerCombatRanged>();
        _meleeAttack = GetComponent<PlayerCombatMelee>();

        _rangedAttack.Initialize(targetLayer);
        _meleeAttack.Initialize(targetLayer);
        parrybox.Initialize(animationController, hurtbox);

        _combatInputEnabled = _parryInputEnabled = true;

        _state.CurrentAction = CombatAction.None;
        _state.Target = Vector3.forward;
        _prevState = _state;
    }
    #endregion

    #region * Update Functions --------------------------------------------------
    // Called by 'Player.cs' in 'Update()' function
    //      - Reads/Records requested input from player
    public void UpdateInput(CombatInput input)
    {
        if (Player.Instance.GetCurrentMovementAction() is not MovementAction.Roll)
        {
            if (_combatInputEnabled || _parryInputEnabled)
            {
                // Parry should only be available if the button is pressed
                //  AND we're not performing a melee attack
                _requestedParry = (input.Parry && _state.CurrentAction is not CombatAction.Melee) || _requestedParry;
            }

            if (_combatInputEnabled)
            {
                // Melee attack should only be available if the button is pressed
                //  AND we're not performing a parry
                _requestedMelee = input.Melee && _state.CurrentAction is not CombatAction.Parry;

                // Ranged attack should only be available if the button is pressed
                //  AND we're not performing a melee attack
                //  AND we're not performing a parry
                _requestedRanged = input.Ranged && _state.CurrentAction is not CombatAction.Melee or CombatAction.Parry;

                _requestedMousePosition = input.MousePosition;
            }
        }
        else
        {
            _requestedRanged = _requestedMelee = false;

            _requestedParry = _parryStarted = false;
        }
    }

    // Called by 'Player.cs' in 'LateUpdate()' function
    //      - Updates current action
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
    #endregion

    #region * PARRY --------------------------------------------------
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
    #endregion

    #region * MELEE --------------------------------------------------
    private void OnMeleeAttack(float deltaTime)
    {
        // Trigger Attack
        _meleeAttack.Attack(ref _state);

        // Update Combat State
    }
    #endregion

    #region * RANGED --------------------------------------------------
    private void OnRangedAttack(float deltaTime)
    {
        // Trigger Attack
        _rangedAttack.Attack(ref _state, deltaTime);

        // Update Combat State
        _state.CurrentAction = !_requestedRanged ? CombatAction.None : _state.CurrentAction;
    }
    #endregion


    #region * Helper Functions --------------------------------------------------
    private void TryEnterNewState()
    {
        _state.CurrentAction = _requestedMelee ? CombatAction.Melee : _requestedRanged ? CombatAction.Ranged : _state.CurrentAction;
    }
    #endregion

    #region * Getters/Setters --------------------------------------------------
    // State Getters
    public CombatState GetState() => _state;
    public CombatState GetPrevState() => _prevState;

    // CombatAction Setters
    public void SetCurrentCombatAction(CombatAction action) => _state.CurrentAction = action;

    // Input Status Getters
    public bool CombatInputEnabled() => _combatInputEnabled;
    public bool ParryInputEnabled() => _parryInputEnabled;
    #endregion
    
    #region * Gateway Functions --------------------------------------------------
    // Combat Input Toggle
    public void CombatInputEnabled(bool b)
    {
        if (!b)
            _combatInputEnabled = _requestedMelee = _requestedRanged = _requestedParry = false;
        else
            _combatInputEnabled = b;
    }

    // Parry Input Toggle
    public void ParryInputEnabled(bool b)
    {
        _parryInputEnabled = b;
    }

    // Reset state machine
    public void ExitCombatState()
    {
        _state.CurrentAction = CombatAction.None;
        _parryStarted = false;
    }
    #endregion
}