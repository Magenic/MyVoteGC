/// <reference path="../_refs.ts" />

module MyVote.Services {
    'use strict';

    export class AuthService {
        private static _zumoUserKey = 'ZUMO_TOKEN';

        //private static _client: Microsoft.WindowsAzure.MobileServiceClient = new WindowsAzure.MobileServiceClient(
        //    'https://myvote.azure-mobile.net/', Globals.zumoKey);

        public static LoginEvent = 'authService.login';

        public static $inject = ['$q', '$rootScope', 'myVoteService', '$firebaseAuth', '$http'];

        public providers: Models.AuthProvider[] = [
            new Models.AuthProvider('twitter', 'Twitter', '/Content/twitter.png', '#01abdf'),
            new Models.AuthProvider('facebook', 'Facebook', '/Content/fb.png', '#3d509f'),
            //new Models.AuthProvider('microsoftaccount', 'Microsoft', '/Content/ms.png', '#fff'),
            new Models.AuthProvider('google', 'Google', '/Content/google.png', '#dc021e')
        ];

        public userId: number;
        public userName: string;
        public emailAddress: string;
        public desiredPath: string;
        public profileId: string;

        public _firebaseAuth: any;
        private _isAuthError: boolean;

        private _q: ng.IQService;
        private _rootScope: ng.IRootScopeService;
        private _myVoteService: MyVoteService;

        private _savedZumoUser: Microsoft.WindowsAzure.User;
        private _http: ng.IHttpService;

        constructor($q: ng.IQService, $rootScope: ng.IRootScopeService, myVoteService: MyVoteService, $firebaseAuth, $http) {
            this._q = $q;
            this._rootScope = $rootScope;
            this._myVoteService = myVoteService;
            this.userId = null;
            this.userName = null;
            this._firebaseAuth = $firebaseAuth;
            this._http = $http;

            var savedUserJSON = window.localStorage[AuthService._zumoUserKey];
            if (savedUserJSON) {
                try {
                    var savedUser = JSON.parse(savedUserJSON);
                    if (savedUser.hasOwnProperty('userId') && savedUser.hasOwnProperty('mobileServiceAuthenticationToken')) {
                        this._savedZumoUser = savedUser;
                        Globals.zumoUserKey = savedUser.mobileServiceAuthenticationToken;
                    } else {
                        window.localStorage.removeItem(AuthService._zumoUserKey);
                    }
                } catch (e) {
                    window.localStorage.removeItem(AuthService._zumoUserKey);
                }
            }
        }

        public foundSavedLogin(): boolean {
            return this._savedZumoUser != null;
        }

        public loadSavedLogin(): ng.IPromise<MyVote.Services.AppServer.Models.User> {
            return this._myVoteService.getUser(this._savedZumoUser.userId)
                .then((result: any) => {
                    if (result) {
                        if (result.UserID == -99) {
                            window.localStorage.removeItem(AuthService._zumoUserKey);
                        }
                        else {
                            this.userId = result.UserID;
                            this.userName = result.UserName;
                            this._rootScope.$emit(AuthService.LoginEvent);
                        }
                    }
                    return result;
                }, (error) => {
                    //Indicate there was an Authorization error and a user was not returned
                    this._isAuthError = true;
                    //Remove any saved user credentials from storage as they were not valid anymore (i.e. expired)
                    window.localStorage.removeItem(AuthService._zumoUserKey);
                    return this._q.reject(error);
                }
                );
        }

        //public login(provider: string): Microsoft.WindowsAzure.asyncPromise {
        //    this.logout();
        //    return AuthService._client.login(provider).then(
        //        result => {
        //            var zumoUser = AuthService._client.currentUser;
        //            this._savedZumoUser = zumoUser;

        //            var zumoUserJSON = JSON.stringify(zumoUser);
        //            window.localStorage[AuthService._zumoUserKey] = zumoUserJSON;
        //            Globals.zumoUserKey = result.mobileServiceAuthenticationToken;

        //            return this._myVoteService.getUser(zumoUser.userId);
        //        }).then((result: MyVote.Services.AppServer.Models.User) => {
        //            if (result) {                        
        //                this.userId = result.UserID;
        //                this.userName = result.UserName;
        //                this._rootScope.$emit(AuthService.LoginEvent);
        //            }
        //            return result;
        //        });
        //}

        public login(provider: string, fbAuth: any): Microsoft.WindowsAzure.asyncPromise {
            var me = this;
            var fbAuthProvider;
            switch (provider) {
                case 'Facebook':
                    fbAuthProvider = new firebase.auth.FacebookAuthProvider();
                    break;
                case 'Twitter':
                    fbAuthProvider = new firebase.auth.TwitterAuthProvider();
                    break;
                default:
                    fbAuthProvider = new firebase.auth.GoogleAuthProvider();
            }


            return fbAuth.$signInWithPopup(fbAuthProvider).then(function (result) {
                // This gives you a Provider Access Token. You can use it to access the additional info.
                var token = result.credential.accessToken;
                // The signed-in user info.
                var user = result.user;
                me.emailAddress = result.user.email;
                var profileId = provider + ":" + user.providerData[0].uid;
                return me._myVoteService.getUser(profileId).then((u) => {
                    return me._http.get(Globals.apiUrl + '/auth/ObtainLocalAccessToken',
                        { params: { provider: provider, userName: profileId } }).success(function (response: any) {

                            var token = response.access_token;
                            var zumoUserJSON = JSON.stringify({ 'mobileServiceAuthenticationToken': token, 'userId': profileId });
                            me.userId = u.UserID;
                            me.userName = u.UserName;
                            me.profileId = profileId;
                            window.localStorage[AuthService._zumoUserKey] = zumoUserJSON;
                            Globals.zumoUserKey = token;
                            me._rootScope.$emit(AuthService.LoginEvent);
                            
                        }).error(function (err, status) {
                            var message = err;
                            //_logOut();
                        });

                });

            }).catch(function (error) {
                // Handle Errors here.
                var errorCode = error.code;
                var errorMessage = error.message;
                // The email of the user's account used.
                var email = error.email;
                // The firebase.auth.AuthCredential type that was used.
                var credential = error.credential;
                // [START_EXCLUDE]
                if (errorCode === 'auth/account-exists-with-different-credential') {
                    alert('You have already signed up with a different auth provider for that email.');
                    // If you are using multiple auth providers on your app you should handle linking
                    // the user's accounts here.
                } else {
                    console.error(error);
                }
            });
        }

        public logout(): void {
            this.userId = null;
            this.userName = null;
            this._savedZumoUser = null;
            Globals.zumoUserKey = null;
            window.localStorage.removeItem(AuthService._zumoUserKey);
            firebase.auth().signOut();
        }

        public isLoggedIn(): boolean {
            return (this.userId != null && this.userId != -99);
        }

        public isAuthError(): boolean {
            return this._isAuthError;
        }

        public isRegistered(): boolean {
            return (this.isLoggedIn() && this.userId != null && this.userId != -99);
        }
		
    }
    App.service('authService', AuthService);
}