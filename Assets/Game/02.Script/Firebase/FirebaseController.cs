using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase;
using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

namespace ThreeMatch.Firebase
{
    public class FirebaseController
    {
        public bool HasUserId => _auth?.CurrentUser != null;
        public string UserId => _auth.CurrentUser?.UserId;
        public FirebaseFirestore DB => _db;
        
        private FirebaseAuth _auth;
        private FirebaseUser _user;
        private FirebaseFirestore _db;

        public async UniTask<bool> FirebaseInit()
        {
            await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread((task) =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Failed firebase app init " + task.Exception?.Message);
                    return false;
                }
                
                FirebaseApp app = FirebaseApp.DefaultInstance;
                _db = FirebaseFirestore.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                return true;
            });

            return _auth != null;
        }
        
        public async UniTask<FirebaseUser> SignInAnonymously()
        {
            FirebaseUser user = null;
            await _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread((task) =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("failed sign in anonymmoulsy");
                    return;
                }

                AuthResult result = task.Result;
                _user = result.User;
            });

            return _user;
        }
        
        public async UniTask SignInGoogle()
        {
            return;
        }
    }
}