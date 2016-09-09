/// <reference path="../_refs.ts" />

module MyVote.Controllers {
    'use strict';

    //Interface extends ng.IScope from angular.d.ts TypeScript definition file
    //This allows referencing properties on the Angular this object without the TypeScript compiler having issues with undefined properties
    export interface LandingScope extends ng.IScope {
        providers: Models.AuthProvider[];
        errorMessage: string;
        busyMessage: string;
        busy: boolean;
        authObj: any;


        login(provider: Models.AuthProvider): void;
        authCompletedCB(provider: string, externalUserName: string, userId: string): void;
    }

    export class LandingCtrl {
        static $inject = ['$scope', '$location', '$log', 'authService', '$window', '$q', '$http', 'myVoteService', '$firebaseAuth'];
        private _myVoteService: Services.MyVoteService;
        constructor(private $scope: LandingScope, $location: ng.ILocationService, $log: ng.ILogService, authService: Services.AuthService, $window, $q, $http, myVoteService: MyVote.Services.MyVoteService, $firebaseAuth) {
            $scope.errorMessage = null;
            $scope.authObj = $firebaseAuth();
            this._myVoteService = myVoteService;
            if (authService.foundSavedLogin()) {
                $log.info('LandingCtrl: auth service found saved login.');

                this.$scope.busyMessage = 'Checking for saved login...';
                this.$scope.busy = true;

                authService.loadSavedLogin().then(
                    login => {
                        if (login && login.UserID != -99) {
                            var targetPath = authService.desiredPath || '/polls';
                            authService.desiredPath = null;
                            $location.path(targetPath).replace();
                        } else {
                            $scope.providers = authService.providers;
                            this.$scope.busyMessage = null;
                            this.$scope.busy = false;
                        }
                    },
                    error => {
                        this.$scope.busyMessage = null;
                        this.$scope.busy = false;
                        this.$scope.errorMessage = error;
                    }
                );
            } else {
                $scope.providers = authService.providers;
                this.$scope.busyMessage = null;
                this.$scope.busy = false;
            }

            var me = this;
            $scope.authCompletedCB = (provider: string, externalUserName: string, profileId: string) => {
                var _zumoUserKey = 'ZUMO_TOKEN';
                me._myVoteService.getUser(profileId).then((u) => {
                    $http.get(Globals.apiUrl + '/auth/ObtainLocalAccessToken',
                        { params: { provider: provider, userName: profileId } }).success(function (response) {
                            var token = response.access_token;
                            var zumoUserJSON = JSON.stringify({ 'mobileServiceAuthenticationToken': token, 'userId': profileId });
                            authService.userId = u.UserID;
                            authService.userName = u.UserName;
                            authService.profileId = profileId;
                            window.localStorage[_zumoUserKey] = zumoUserJSON;

                            var savedUserJSON = window.localStorage[_zumoUserKey];
                            Globals.zumoUserKey = token;

                            var registered = authService.isRegistered();
                            $log.info('LandingCtrl: login complete. registered: ', registered);

                            // If user id is not found, the app server will return a -99
                            var navPath = u.UserID == -99 ? '/registration' : '/polls';
                            $location.path(navPath)
                        }).error(function (err, status) {
                            var message = err;
                        });

                });
            }

            $scope.login = (provider: Models.AuthProvider) => {
                if ($scope.busy) {
                    return;
                }
                $scope.busyMessage = 'Logging in...';
                $scope.busy = true;
                $scope.errorMessage = null;

                authService.login(provider.name, $scope.authObj).then(() => {
                    $scope.busy = false;
                    $scope.busyMessage = null;
                    var registered = authService.isRegistered();
                    $log.info('LandingCtrl: login complete. registered: ', registered);

                    var navPath = registered ? '/polls' : '/registration';
                    $location.path(navPath);
                }),
                    error => $scope.$apply(() => {
                        $log.error('LandingCtrl: login error: ', error);

                        $scope.busyMessage = null;
                        $scope.busy = false;
                        $scope.errorMessage = error;
                    });

                return;
            };
        }
    }
}
declare var firebase: any;
