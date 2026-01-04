using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{	
		[Header("Mobile Inputs")]
        public FixedJoystick moveJoystick; 
        public FixedJoystick lookJoystick; 
		public bool isMobile = true;
        
        [Header("Mobile Sprint UX")]
        public RectTransform sprintIndicator; // Koşu görselinin RectTransform'u
        [Tooltip("Joystick'in bu değere ulaştığında otomatik koşmayı tetikler")]
        public float AutoSprintThreshold = 0.9f;
		[Header("Joystick Durumları")]
        public bool isLooking = false; // Bakış joystick'i şu an kullanılıyor mu?
		[Header("Giriş Ayarları")]
		// **public** olarak tanımlandığından emin olun
		public StarterAssetsInputs starterInputs; 
		public float runThreshold = 0.8f;

		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		
		[Header("Game Actions")]
        public bool attack; 
        public bool die;
		public bool interact;
		public bool inventory;

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
			if(cursorInputForLook)
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
		
        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }

        public void OnDie(InputValue value)
        {
            DieInput(value.isPressed);
        }

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

		public void OnInventory(InputValue value)
		{
			InventoryInput(value.isPressed);
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
		// ✨ EKSİK: Saldırı Input Helper Metodu
        public void AttackInput(bool newAttackState)
        {
            attack = newAttackState;
        }

        // ✨ EKSİK: Ölme Input Helper Metodu
        public void DieInput(bool newDieState)
        {
            die = newDieState;
        }
		public void InteractInput(bool newInteractState)
		{
			interact = newInteractState;
		}
		public void InventoryInput(bool newInventoryState)
		{
			inventory = newInventoryState;
		}
	
        public void SetIsLooking(bool newState)
        {
            isLooking = newState;
        }
		private void ReadMobileInput()
		{
			// 1. HAREKET (MOVE & SPRINT)
			if (moveJoystick != null)
			{
				Vector2 moveDirection = moveJoystick.Direction;
				MoveInput(moveDirection); 

				bool shouldSprint = false;

				// Otomatik Koşu Mantığı
				if (moveDirection.magnitude >= AutoSprintThreshold)
				{
					shouldSprint = true;
					// Koşu İkonu görselleştirmesi
					if (sprintIndicator != null)
					{
						sprintIndicator.gameObject.SetActive(true); 
					}
				}
				else if (sprintIndicator != null)
				{
					sprintIndicator.gameObject.SetActive(false);
				}
				
				SprintInput(shouldSprint);
			}
			
			// 2. KAMERA (LOOK)
			if (lookJoystick != null)
			{
				LookInput(lookJoystick.Direction); 
			}
		}

		// ✨ Her karede mobil girdiyi okumak için
		private void Update()
		{
			if (isMobile) 
    		{
        		ReadMobileInput();
    		}
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