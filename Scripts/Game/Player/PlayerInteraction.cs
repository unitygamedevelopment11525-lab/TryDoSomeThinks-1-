using Project.Scripts.Game.Inventory;
using UnityEngine;
using UnityEngine.InputSystem; 
namespace Project.Scripts.Game.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        public float interactionDistance = 3f;
        public LayerMask interactionLayer;
     
        private InputSystem_Actions _inputActions;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
        }
        
        private void OnEnable()
        {
            _inputActions.Player.Enable();
            // Використовуємо .started замість .performed для миттєвої реакції
            _inputActions.Player.Interact.started += OnInteractPressed;
        }

        private void OnDisable()
        {
            _inputActions.Player.Interact.started -= OnInteractPressed;
            _inputActions.Player.Disable();
        }


        private void OnInteractPressed(InputAction.CallbackContext context)
        {
            Debug.Log("Interact");
            TryInteract();
        }

        private void TryInteract()
        {
            Debug.Log("Try Interact");
            
            // Використовуємо позицію та напрямок погляду камери замість тіла гравця
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : transform;
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
            {
                Debug.Log("Ray cast Hit: " +  hit.collider.gameObject.name); 
                
                ItemObject itemObject = hit.collider.GetComponent<ItemObject>();
                
                if (itemObject != null)
                {
                    Debug.Log("Item find");
                    itemObject.PickUp();
                }
            }
        }
    }
}