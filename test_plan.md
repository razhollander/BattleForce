1. Modify `PlayerEyesView.cs`:
   - Add a private boolean field: `private bool _isLockOnAngry;`
   - Add a method `SetIsLockOnAngry(bool isAngry)` which updates `_isLockOnAngry` and then sets the active state of the eyes.
   - Refactor the toggling logic into a helper method, like `UpdateEyeState()`:
     ```csharp
     private bool _isShortDurationAngry;
     ```
     Wait, we can just use `_isShortDurationAngry` and `_isLockOnAngry`.
     ```csharp
     private bool _isShortDurationAngry;
     private bool _isLockOnAngry;

     public void SetIsLockOnAngry(bool isLockOnAngry)
     {
         _isLockOnAngry = isLockOnAngry;
         UpdateEyeState();
     }

     private void UpdateEyeState()
     {
         bool shouldBeAngry = _isShortDurationAngry || _isLockOnAngry;
         _angryLeftEye.TrySetActive(shouldBeAngry);
         _angryRightEye.TrySetActive(shouldBeAngry);
         _leftEye.gameObject.TrySetActive(!shouldBeAngry);
         _rightEye.gameObject.TrySetActive(!shouldBeAngry);
     }
     ```
   - Update `MakeAngryForShortDuration` and `SetAngryForShortDurationAsync`:
     ```csharp
     public void MakeAngryForShortDuration(CancellationToken cancellationToken)
     {
         DisableSpinned();
         _angryEyesCancellationTokenSource?.Cancel();
         _angryEyesCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
         SetAngryForShortDurationAsync(_angryEyesCancellationTokenSource.Token).Forget();
     }

     private async Awaitable SetAngryForShortDurationAsync(CancellationToken cancellationToken)
     {
         _isShortDurationAngry = true;
         UpdateEyeState();

         try
         {
             await Awaitable.WaitForSecondsAsync(_angryDurationInSeconds, cancellationToken);
         }
         finally
         {
             _isShortDurationAngry = false;
             UpdateEyeState();
         }
     }
     ```
   - Reset `_isLockOnAngry` and `_isShortDurationAngry` in `OnDespawned()` just to be safe.
     ```csharp
     public void OnDespawned()
     {
         DisableSpinned();
         _isLockOnAngry = false;
         _isShortDurationAngry = false;
         UpdateEyeState();
     }
     ```

2. Modify `MatchPlayerView.cs`:
   - In `SetIsLockOnHeartSightShown(bool isShown)`:
     ```csharp
     public void SetIsLockOnHeartSightShown(bool isShown)
     {
         _lockOnHeartSightView.SetIsShown(isShown);
         _playerEyesView.SetIsLockOnAngry(isShown);
     }
     ```

3. Ensure Pre-commit tasks are executed.

4. Submit
