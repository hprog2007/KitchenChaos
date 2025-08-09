using System;
using UnityEngine;
using static CuttingCounter;

public class ToasterCounter : BaseCounter, IHasProgress {

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs {
        public State state;
    }

    public enum State {
        Idle,
        Toasting,
        Poped,
        Burned,
    }

    [SerializeField] private ToastingRecipeSO[] toastingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State state;
    private float toastingTimer;
    private ToastingRecipeSO toastingRecipeSO;
    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    private void Start() {
        state = State.Idle;
    }

    private void Update() {

        if (HasKitchenObject()) {
            switch (state) {
                case State.Idle:
                    break;
                case State.Toasting:
                    toastingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = toastingTimer / toastingRecipeSO.toastingTimerMax
                    });

                    if (toastingTimer > toastingRecipeSO.toastingTimerMax) {
                        //Poped
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(toastingRecipeSO.output, this);

                        state = State.Poped;
                        burningTimer = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                            state = state
                        });

                    }
                    break;
                case State.Poped:
                    /*
                     * 
                     * 
                    burningTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                    });

                    if (burningTimer > burningRecipeSO.burningTimerMax) {
                        //Poped
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                            state = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                    }
                    */
                    break;
                case State.Burned:
                    break;
            }
        }
    }

    public override void Interact(Player player) {
        //if player hasn't put breads on ToasterCounter 
        if (!HasKitchenObject()) {
            //There is no kitchen object here
            if (player.HasKitchenObject()) {
                //The player is carrying something
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
                    //The player is carrying something that can be Poped
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    toastingRecipeSO = GetToastingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    state = State.Toasting;
                    toastingTimer = 0f;



                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = toastingTimer / toastingRecipeSO.toastingTimerMax
                    });

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                        state = state
                    });
                }
            } else {
                //The player not carrying anything
            }
        } else {
            //There is a kitchen object here
            if (player.HasKitchenObject()) {
                //The player carrying something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    //Player is holding a plate                    
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                        GetKitchenObject().DestroySelf();

                        state = State.Idle;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                            state = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                    }
                }
            } else {
                //The player not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);

                state = State.Idle;

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                    state = state
                });

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                    progressNormalized = 0f
                });
            }

        }
    }


    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        ToastingRecipeSO toastingRecipeSO = GetToastingRecipeSOWithInput(inputKitchenObjectSO);

        return toastingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
        ToastingRecipeSO toastingRecipeSO = GetToastingRecipeSOWithInput(inputKitchenObjectSO);

        if (toastingRecipeSO != null) {
            return toastingRecipeSO.output;
        } else {
            return null;
        }
    }

    private ToastingRecipeSO GetToastingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (ToastingRecipeSO toastingRecipeSO in toastingRecipeSOArray) {
            if (toastingRecipeSO.input == inputKitchenObjectSO) {
                return toastingRecipeSO;
            }
        }

        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray) {
            if (burningRecipeSO.input == inputKitchenObjectSO) {
                return burningRecipeSO;
            }
        }

        return null;
    }

    public bool IsPoped() {
        return state == State.Poped;
    }
}
