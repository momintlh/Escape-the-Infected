using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {

        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool interact;
        public bool interaction;
        public bool useItem;
        public bool slot1;
        public bool slot2;

        public event EventHandler OnInteractPlayer;
        public event EventHandler OnInteractionPlayer;
        public event EventHandler OnUseItemPlayer;
        public event EventHandler OnSlotChange1;
        public event EventHandler OnSlotChange2;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                OnInteractPlayer?.Invoke(this, EventArgs.Empty);
            }

        }
        public void OnInteraction(InputValue value)
        {
            if (value.isPressed)
            {
                OnInteractionPlayer?.Invoke(this, EventArgs.Empty);
            }
        }
        public void OnUseItem(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("Mouse is being pressed");
                OnUseItemPlayer?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OnSlot1(InputValue value)
        {
            if (value.isPressed)
            {
                OnSlotChange1?.Invoke(this, EventArgs.Empty);
            }
        }
        public void OnSlot2(InputValue value)
        {
            if (value.isPressed)
            {
                OnSlotChange2?.Invoke(this, EventArgs.Empty);
            }
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void InteractionInput(bool newInteractionState)
        {
            interaction = newInteractionState;
        }

        public void UseItem(bool newUseItem)
        {
            useItem = newUseItem;
        }

        public void Slot1(bool newSlot)
        {
            slot1 = newSlot;
        }  
        public void Slot2(bool newSlot)
        {
            slot2 = newSlot;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

}