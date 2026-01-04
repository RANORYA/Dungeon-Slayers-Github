using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {   
        public GameManager gameManager;

        [Header("Combat Settings")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public bool isDead = false;

        [Header("Interact Settings")]
        [Tooltip("Etkileşimli objelerin bulunduğu Layer Mask.")]
        public LayerMask interactable;
        private UniversalLootChest _currentChest = null;
        // Karakterin önünde algılanan mevcut etkileşimli obje
        private System.Collections.Generic.List<Interactable> _interactablesInRange = new System.Collections.Generic.List<Interactable>();

        [Header("Vault Settings")]
        public float vaultDetectionDistance = 0.5f;
        [Tooltip("Sadece bu katmandaki objeler atlanabilir engel olarak algılanır.")]
        public LayerMask vaultableLayer;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDAttack;
        private int _animIDDie;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input; 
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        [Header("Custom Actions")]
        public float attackDamage = 25f;
        public float attackRange = 2.5f;
        public Slider playerHealthSlider;

        [Header("UI Settings")]
        public GameObject lootButtonObj;
        
        
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void CheckForVault()
        {
            RaycastHit hit;
            Vector3 startPosition = transform.position + transform.forward * _controller.radius;
            Vector3 origin = startPosition + Vector3.up * 0.5f; 
            
            if (Physics.Raycast(origin, transform.forward, out hit, vaultDetectionDistance, vaultableLayer))
            {
                if (Grounded && _jumpTimeoutDelta <= 0.0f)
                {
                    _jumpTimeoutDelta = JumpTimeout; 
                    _input.JumpInput(false);
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }
            }
        }
        
        private void Awake()
        {   
            _animator = GetComponent<Animator>();
            _input = GetComponent<StarterAssetsInputs>();
            
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {   
            playerHealthSlider.maxValue = maxHealth;
            playerHealthSlider.value = currentHealth;            
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            // Sadece 'isDead' kullanıyoruz
            if (isDead)
            {
                return; 
            }

            CheckForVault();
            JumpAndGravity();
            GroundedCheck();
            HandleCombat(); 
            HandleInteract();
            Move();
            CheckLootButtonVisibility(); // Her karede kontrol et
            
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void CheckLootButtonVisibility()
        {
            float detectionRadius = 2.0f; 
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, interactable);

            bool foundValidChest = false;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out UniversalLootChest chest))
                {
                    // Eğer sandık hala sahnede aktifse
                    if (chest.gameObject.activeInHierarchy)
                    {
                        _currentChest = chest;
                        foundValidChest = true;
                        break;
                    }
                }
            }

            if (!foundValidChest)
            {
                _currentChest = null;
            }

            if (lootButtonObj != null)
            {
                lootButtonObj.SetActive(foundValidChest);
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDAttack = Animator.StringToHash("Attack"); 
            _animIDDie = Animator.StringToHash("Die");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // ⭐ KRİTİK YENİ KONTROL: Eğer sağ joystick aktifse (isLooking = true), hareket girdisini zorla sıfırla.
            if (_input.isLooking)
            {
                // Karakterin tüm hareket girdisini anında sıfırla.
                _input.MoveInput(Vector2.zero);
                // Bu, karakterin hemen durmasını ve sadece kameranın dönmesini sağlar.
            }
            // 🚫 Önceki isLooking kontrolü kaldırıldı. Artık sadece hareket rotasyonu kontrol edilecek.
            
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move.sqrMagnitude < _threshold * _threshold)
                {
                    targetSpeed = 0.0f;
                }
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            // ✨ GÜNCELLEME: Floating point hatalarını önlemek için Vector2.zero yerine eşik (threshold) kontrolü kullanıldı.
            if (_input.move.sqrMagnitude >= _threshold * _threshold)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
        
        private void HandleCombat()
        {
            if (_input.attack)
            {
                if (_hasAnimator && !isDead)
                {
                    // YENİ: Saldırmadan önce en yakın düşmana dön
                    FaceClosestEnemy();

                    _animator.SetTrigger(_animIDAttack);
                }
                _input.attack = false;
            }
        }

        private void FaceClosestEnemy()
        {
            float detectionRadius = 3f; // Düşman algılama mesafesi
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
            Transform closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = hitCollider.transform;
                    }
                }
            }

            if (closestEnemy != null)
            {
                // Sadece Y ekseninde (sağa-sola) dönmesi için
                Vector3 targetDirection = closestEnemy.position - transform.position;
                targetDirection.y = 0; // Karakterin yere paralel kalmasını sağlar
                
                if (targetDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    // Karakteri anında düşmana döndürür
                    transform.rotation = targetRotation;
                }
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void HandleInteract()
        {
            // 1. Etkileşim butonu basılı mı?
            if (_input.interact)
            {
                // 2. Menzilde bir sandık bulundu mu?
                if (_currentChest != null)
                {
                    // ⭐ DÜZELTME: Senin scriptindeki fonksiyonun adı 'Interact'
                    // Ve bu fonksiyon parametre olarak 'this' (yani controller'ın kendisini) bekliyor
                    _currentChest.Interact(this); 
                    
                    // Etkileşim başladığı için referansı temizle ve butonu kapat
                    _currentChest = null;
                    if (lootButtonObj != null) lootButtonObj.SetActive(false);
                }
                
                // Giriş komutunu sıfırla
                _input.InteractInput(false);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit) {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic) return;
            if (hit.moveDirection.y < -0.3f) return;

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.AddForce(pushDir * 1.1f, ForceMode.Impulse);
        }
    
        public void TakeDamage(float damage) 
        {
            if (isDead) return;

            // Zırh hesaplama: Hasarın en az %10'u her zaman geçer (opsiyonel kural)
            int armor = GetTotalArmor();
            float finalDamage = Mathf.Max(damage - armor, damage * 0.1f); 

            currentHealth -= finalDamage;
            
            if (playerHealthSlider != null) 
            {
                playerHealthSlider.value = currentHealth;
            }

            if (currentHealth <= 0) 
            {
                currentHealth = 0;
                Die();
            }
        }

        private void Die() 
        {
            if (isDead) return;
            isDead = true;

            Debug.Log("Ölüm fonksiyonu çalıştı!"); // Konsolu kontrol et

            if (_hasAnimator) _animator.SetTrigger("Die"); 
            
            // Karakteri durdur
            if (_controller != null) _controller.enabled = false;

            // GameManager kontrolü
            if (gameManager != null)
            {
                gameManager.ShowGameOver();
            }
            else
            {
                // Eğer Inspector'da GameManager sürüklenmemişse hata verir:
                Debug.LogError("DİKKAT: ThirdPersonController içindeki 'Game Manager' kutusu boş!");
                // Alternatif çözüm: Kod ile bulmayı dene
                FindObjectOfType<GameManager>()?.ShowGameOver();
            }
        }
        public int GetTotalDamage()
        {
            float total = attackDamage; // Karakterin çıplak el hasarı
            
            // EquipmentManager'daki silah slotuna (index 3 veya Weapon enum) bakıyoruz
            // Not: EquipSlot.Weapon enum değerini int'e çevirip diziden çekiyoruz
            var weapon = EquipmentManager.Instance.GetCurrentItem(EquipSlot.Weapon);
            if (weapon != null)
            {
                total += weapon.damageBonus;
            }
            return Mathf.RoundToInt(total);
        }
        public int GetTotalArmor()
        {
            int totalArmor = 0;
            // Tüm slotlardaki zırh bonuslarını topla
            foreach (EquipSlot slot in System.Enum.GetValues(typeof(EquipSlot)))
            {
                var item = EquipmentManager.Instance.GetCurrentItem(slot);
                if (item != null)
                {
                    totalArmor += item.armorBonus;
                }
            }
            return totalArmor;
        }

        public void NewEvent() 
        {
            if (isDead) return;

            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up; 
            
            Debug.DrawRay(rayOrigin, transform.forward * attackRange, Color.red, 1.0f);

            if (Physics.Raycast(rayOrigin, transform.forward, out hit, attackRange))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    // ARTIK GetTotalDamage() kullanarak toplam hasarı veriyoruz
                    int totalAtk = GetTotalDamage();
                    hit.collider.GetComponent<EnemyController>()?.TakeDamage(totalAtk);
                    Debug.Log($"Düşmana {totalAtk} toplam hasar verildi! (Bonus dahil)");
                }
            }
        }
    }

}