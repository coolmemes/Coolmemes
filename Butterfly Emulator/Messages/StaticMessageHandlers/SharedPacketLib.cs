namespace Butterfly.Messages.StaticMessageHandlers
{
    class SharedPacketLib
    {
        internal static void CheckHabboRelease(GameClientMessageHandler handler)
        {
            handler.CheckRelease();
        }

        internal static void InitCrypto(GameClientMessageHandler handler)
        {
            handler.InitCrypto();
        }

        internal static void SitDown(GameClientMessageHandler handler)
        {
            handler.SitDown();
        }

        internal static void SecretKey(GameClientMessageHandler handler)
        {
            handler.InitSecretKey();
        }

        internal static void setVars(GameClientMessageHandler handler)
        {
            handler.setClientVars();
        }

        internal static void setUniqueId(GameClientMessageHandler handler)
        {
            handler.setUniqueIDToClient();
        }

        internal static void SendCampaingData(GameClientMessageHandler handler)
        {
            handler.SendCampaingData();
        }

        internal static void GetCatalogIndex(GameClientMessageHandler handler)
        {
            handler.GetCatalogIndex();
        }

        internal static void GetCatalogPage(GameClientMessageHandler handler)
        {
            handler.GetCatalogPage();
        }

        internal static void RedeemVoucher(GameClientMessageHandler handler)
        {
            handler.RedeemVoucher();
        }

        internal static void HandlePurchase(GameClientMessageHandler handler)
        {
            handler.HandlePurchase();
        }

        internal static void PurchaseGift(GameClientMessageHandler handler)
        {
            handler.PurchaseGift();
        }

        internal static void GetCataData1(GameClientMessageHandler handler)
        {
            handler.GetCataData1();
        }

        internal static void MarketplaceCanSell(GameClientMessageHandler handler)
        {
            handler.MarketplaceCanSell();
        }

        internal static void MarketplaceSetPrice(GameClientMessageHandler handler)
        {
            handler.MarketplaceSetPrice();
        }

        internal static void MarketplacePostItem(GameClientMessageHandler handler)
        {
            handler.MarketplacePostItem();
        }

        internal static void MarketplaceGetOwnOffers(GameClientMessageHandler handler)
        {
            handler.MarketplaceGetOwnOffers();
        }

        internal static void MarketplaceGetOffers(GameClientMessageHandler handler)
        {
            handler.MarketplaceGetOffers();
        }

        internal static void MarketplacePurchase(GameClientMessageHandler handler)
        {
            handler.MarketplacePurchase();
        }

        internal static void MarketplaceTakeBack(GameClientMessageHandler handler)
        {
            handler.MarketplaceTakeBack();
        }

        internal static void MarketplaceClaimCredits(GameClientMessageHandler handler)
        {
            handler.MarketplaceClaimCredits();
        }

        internal static void CheckPetName(GameClientMessageHandler handler)
        {
            handler.CheckPetName();
        }

        internal static void RemoveHanditem(GameClientMessageHandler handler)
        {
            handler.RemoveHanditem();
        }

        internal static void GiveHanditem(GameClientMessageHandler handler)
        {
            handler.GiveHanditem();
        }

        internal static void SSOLogin(GameClientMessageHandler handler)
        {
            handler.SSOLogin();
        }

        internal static void SubmitHelpTicket(GameClientMessageHandler handler)
        {
            handler.SubmitHelpTicket();
        }

        internal static void ModGetUserInfo(GameClientMessageHandler handler)
        {
            handler.ModGetUserInfo();
        }

        internal static void ModGetUserChatlog(GameClientMessageHandler handler)
        {
            handler.ModGetUserChatlog();
        }

        internal static void ModGetRoomChatlog(GameClientMessageHandler handler)
        {
            handler.ModGetRoomChatlog();
        }

        internal static void ModGetRoomTool(GameClientMessageHandler handler)
        {
            handler.ModGetRoomTool();
        }

        internal static void ModPickTicket(GameClientMessageHandler handler)
        {
            handler.ModPickTicket();
        }

        internal static void ModReleaseTicket(GameClientMessageHandler handler)
        {
            handler.ModReleaseTicket();
        }

        internal static void ModCloseTicket(GameClientMessageHandler handler)
        {
            handler.ModCloseTicket();
        }

        internal static void ModGetTicketChatlog(GameClientMessageHandler handler)
        {
            handler.ModGetTicketChatlog();
        }

        internal static void ModGetRoomVisits(GameClientMessageHandler handler)
        {
            handler.ModGetRoomVisits();
        }

        internal static void ModSendRoomAlert(GameClientMessageHandler handler)
        {
            handler.ModSendRoomAlert();
        }
        internal static void RequestHelp(GameClientMessageHandler handler)
        {
            handler.RequestHelp();
        }
        internal static void ModPerformRoomAction(GameClientMessageHandler handler)
        {
            handler.ModPerformRoomAction();
        }

        internal static void ModSendUserMessage(GameClientMessageHandler handler)
        {
            handler.ModSendUserMessage();
        }

        internal static void ModKickUser(GameClientMessageHandler handler)
        {
            handler.ModKickUser();
        }

        internal static void ModMuteUser(GameClientMessageHandler handler)
        {
            handler.ModMuteUser();
        }

        internal static void ModBanUser(GameClientMessageHandler handler)
        {
            handler.ModBanUser();
        }

        internal static void RemoveBuddy(GameClientMessageHandler handler)
        {
            handler.RemoveBuddy();
        }

        internal static void SearchHabbo(GameClientMessageHandler handler)
        {
            handler.SearchHabbo();
        }

        internal static void AcceptRequest(GameClientMessageHandler handler)
        {
            handler.AcceptRequest();
        }

        internal static void DeclineRequest(GameClientMessageHandler handler)
        {
            handler.DeclineRequest();
        }

        internal static void RequestBuddy(GameClientMessageHandler handler)
        {
            handler.RequestBuddy();
        }

        internal static void SendInstantMessenger(GameClientMessageHandler handler)
        {
            handler.SendInstantMessenger();
        }

        internal static void FollowBuddy(GameClientMessageHandler handler)
        {
            handler.FollowBuddy();
        }

        internal static void SendInstantInvite(GameClientMessageHandler handler)
        {
            handler.SendInstantInvite();
        }

        internal static void AddFavorite(GameClientMessageHandler handler)
        {
            handler.AddFavorite();
        }

        internal static void RemoveFavorite(GameClientMessageHandler handler)
        {
            handler.RemoveFavorite();
        }

        internal static void GoToHotelView(GameClientMessageHandler handler)
        {
            handler.GoToHotelView();
        }

        internal static void PromotedGetMyRooms(GameClientMessageHandler handler)
        {
            handler.PromotedGetMyRooms();
        }

        internal static void StartEvent(GameClientMessageHandler handler)
        {
            handler.StartEvent();
        }

        internal static void EditEvent(GameClientMessageHandler handler)
        {
            handler.EditEvent();
        }

        internal static void GetFlatCats(GameClientMessageHandler handler)
        {
            handler.GetFlatCats();
        }

        internal static void GetInventory(GameClientMessageHandler handler)
        {
            handler.GetInventory();
        }

        internal static void GetRoomData2(GameClientMessageHandler handler)
        {
            handler.GetRoomData2();
        }

        internal static void GetRoomData3(GameClientMessageHandler handler)
        {
            handler.GetRoomData3();
        }

        internal static void ReqLoadRoomForUser(GameClientMessageHandler handler)
        {
            handler.ReqLoadRoomForUser();
        }

        internal static void enterOnRoom(GameClientMessageHandler handler)
        {
            handler.enterOnRoom();
        }

        internal static void enterOnRoom2(GameClientMessageHandler handler)
        {
            handler.enterOnRoom2();
        }

        internal static void ClearRoomLoading(GameClientMessageHandler handler)
        {
            handler.ClearRoomLoading();
        }

        internal static void Talk(GameClientMessageHandler handler)
        {
            handler.Talk();
        }

        internal static void Shout(GameClientMessageHandler handler)
        {
            handler.Shout();
        }

        internal static void Whisper(GameClientMessageHandler handler)
        {
            handler.Whisper();
        }

        internal static void Move(GameClientMessageHandler handler)
        {
            handler.Move();
        }

        internal static void CanCreateRoom(GameClientMessageHandler handler)
        {
            handler.CanCreateRoom();
        }

        internal static void CreateRoom(GameClientMessageHandler handler)
        {
            handler.CreateRoom();
        }

        internal static void GetRoomEditData(GameClientMessageHandler handler)
        {
            handler.GetRoomEditData();
        }

        internal static void SaveRoomData(GameClientMessageHandler handler)
        {
            handler.SaveRoomData();
        }

        internal static void RelaodRights(GameClientMessageHandler handler)
        {
            handler.ReloadRights();
        }

        internal static void GiveRights(GameClientMessageHandler handler)
        {
            handler.GiveRights();
        }

        internal static void TakeRights(GameClientMessageHandler handler)
        {
            handler.TakeRights();
        }

        internal static void TakeAllRights(GameClientMessageHandler handler)
        {
            handler.TakeAllRights();
        }

        internal static void KickUser(GameClientMessageHandler handler)
        {
            handler.KickUser();
        }

        internal static void BanUser(GameClientMessageHandler handler)
        {
            handler.BanUser();
        }

        internal static void MuteUser(GameClientMessageHandler handler)
        {
            handler.MuteUser();
        }

        internal static void MuteRoom(GameClientMessageHandler handler)
        {
            handler.MuteRoom();
        }

        internal static void ReloadBans(GameClientMessageHandler handler)
        {
            handler.ReloadBans();
        }

        internal static void RemoveBans(GameClientMessageHandler handler)
        {
            handler.RemoveBans();
        }

        internal static void SetHomeRoom(GameClientMessageHandler handler)
        {
            handler.SetHomeRoom();
        }

        internal static void DeleteRoom(GameClientMessageHandler handler)
        {
            handler.DeleteRoom();
        }

        internal static void LookAt(GameClientMessageHandler handler)
        {
            handler.LookAt();
        }

        internal static void StartTyping(GameClientMessageHandler handler)
        {
            handler.StartTyping();
        }

        internal static void StopTyping(GameClientMessageHandler handler)
        {
            handler.StopTyping();
        }

        internal static void IgnoreUser(GameClientMessageHandler handler)
        {
            handler.IgnoreUser();
        }

        internal static void UnignoreUser(GameClientMessageHandler handler)
        {
            handler.UnignoreUser();
        }

        internal static void Wave(GameClientMessageHandler handler)
        {
            handler.Wave();
        }

        internal static void Sign(GameClientMessageHandler handler)
        {
            handler.Sign();
        }

        internal static void GetUserBadges(GameClientMessageHandler handler)
        {
            handler.GetUserBadges();
        }

        internal static void RateRoom(GameClientMessageHandler handler)
        {
            handler.RateRoom();
        }

        internal static void Dance(GameClientMessageHandler handler)
        {
            handler.Dance();
        }

        internal static void AnswerDoorbell(GameClientMessageHandler handler)
        {
            handler.AnswerDoorbell();
        }

        internal static void ApplyRoomEffect(GameClientMessageHandler handler)
        {
            handler.ApplyRoomEffect();
        }

        internal static void PlacePostIt(GameClientMessageHandler handler)
        {
            handler.PlacePostIt();
        }

        internal static void PlaceItem(GameClientMessageHandler handler)
        {
            handler.PlaceItem();
        }

        internal static void TakeItem(GameClientMessageHandler handler)
        {
            handler.TakeItem();
        }

        internal static void MoveItem(GameClientMessageHandler handler)
        {
            handler.MoveItem();
        }

        internal static void MoveWallItem(GameClientMessageHandler handler)
        {
            handler.MoveWallItem();
        }

        internal static void TriggerItem(GameClientMessageHandler handler)
        {
            handler.TriggerItem();
        }

        internal static void TriggerItemDiceSpecial(GameClientMessageHandler handler)
        {
            handler.TriggerItemDiceSpecial();
        }

        internal static void OpenPostit(GameClientMessageHandler handler)
        {
            handler.OpenPostit();
        }

        internal static void SavePostit(GameClientMessageHandler handler)
        {
            handler.SavePostit();
        }

        internal static void DeletePostit(GameClientMessageHandler handler)
        {
            handler.DeletePostit();
        }

        internal static void OpenPresent(GameClientMessageHandler handler)
        {
            handler.OpenPresent();
        }

        internal static void GetMoodlight(GameClientMessageHandler handler)
        {
            handler.GetMoodlight();
        }

        internal static void UpdateMoodlight(GameClientMessageHandler handler)
        {
            handler.UpdateMoodlight();
        }

        internal static void SwitchMoodlightStatus(GameClientMessageHandler handler)
        {
            handler.SwitchMoodlightStatus();
        }

        internal static void InitTrade(GameClientMessageHandler handler)
        {
            handler.InitTrade();
        }

        internal static void OfferTradeItem(GameClientMessageHandler handler)
        {
            handler.OfferTradeItem();
        }

        internal static void OfferTradeMultiItem(GameClientMessageHandler handler)
        {
            handler.OfferTradeMultiItem();
        }

        internal static void TakeBackTradeItem(GameClientMessageHandler handler)
        {
            handler.TakeBackTradeItem();
        }

        internal static void StopTrade(GameClientMessageHandler handler)
        {
            handler.StopTrade();
        }

        internal static void AcceptTrade(GameClientMessageHandler handler)
        {
            handler.AcceptTrade();
        }

        internal static void UnacceptTrade(GameClientMessageHandler handler)
        {
            handler.UnacceptTrade();
        }

        internal static void CompleteTrade(GameClientMessageHandler handler)
        {
            handler.CompleteTrade();
        }

        internal static void GiveRespect(GameClientMessageHandler handler)
        {
            handler.GiveRespect();
        }

        internal static void ApplyEffect(GameClientMessageHandler handler)
        {
            handler.ApplyEffect();
        }

        internal static void EnableEffect(GameClientMessageHandler handler)
        {
            handler.EnableEffect();
        }

        internal static void RedeemExchangeFurni(GameClientMessageHandler handler)
        {
            handler.RedeemExchangeFurni();
        }

        internal static void KickBot(GameClientMessageHandler handler)
        {
            handler.KickBot();
        }

        internal static void PlacePet(GameClientMessageHandler handler)
        {
            handler.PlacePet();
        }

        internal static void GetPetInfo(GameClientMessageHandler handler)
        {
            handler.GetPetInfo();
        }

        internal static void PickUpPet(GameClientMessageHandler handler)
        {
            handler.PickUpPet();
        }

        internal static void RespectPet(GameClientMessageHandler handler)
        {
            handler.RespectPet();
        }

        internal static void AddSaddle(GameClientMessageHandler handler)
        {
            handler.AddSaddle();
        }

        internal static void RemoveSaddle(GameClientMessageHandler handler)
        {
            handler.RemoveSaddle();
        }

        internal static void Ride(GameClientMessageHandler handler)
        {
            handler.MountOnPet();
        }

        internal static void AllCanMount(GameClientMessageHandler handler)
        {
            handler.AllCanMount();
        }

        internal static void Unride(GameClientMessageHandler handler)
        {
            handler.CancelMountOnPet();
        }

        internal static void CommandsPet(GameClientMessageHandler handler)
        {
            handler.CommandsPet();
        }

        internal static void PetRaces(GameClientMessageHandler handler)
        {
            handler.PetRaces();
        }

        internal static void SaveWired(GameClientMessageHandler handler)
        {
            handler.SaveWired();
        }

        internal static void SaveWiredCondition(GameClientMessageHandler handler)
        {
            handler.SaveWiredConditions();
        }

        public static void GetMusicData(GameClientMessageHandler Handler)
        {
            Handler.GetMusicData();
        }

        public static void LoadInvSongs(GameClientMessageHandler Handler)
        {
            Handler.LoadInvSongs();
        }

        public static void LoadJukeSongs(GameClientMessageHandler Handler)
        {
            Handler.LoadJukeSongs();
        }

        public static void AddNewCdToJuke(GameClientMessageHandler Handler)
        {
            Handler.AddNewCdToJuke();
        }

        public static void RemoveCdToJuke(GameClientMessageHandler Handler)
        {
            Handler.RemoveCdToJuke();
        }

        internal static void LoadProfile(GameClientMessageHandler handler)
        {
            handler.LoadProfile();
        }

        internal static void GetBalance(GameClientMessageHandler handler)
        {
            handler.GetBalance();
        }

        internal static void GetBadges(GameClientMessageHandler handler)
        {
            handler.GetBadges();
        }

        internal static void UpdateBadges(GameClientMessageHandler handler)
        {
            handler.UpdateBadges();
        }
        internal static void attTags(GameClientMessageHandler handler)
        {
            handler.attTags();
        }
        internal static void GetAchievements(GameClientMessageHandler handler)
        {
            handler.GetAchievements();
        }

        internal static void ChangeLook(GameClientMessageHandler handler)
        {
            handler.ChangeLook();
        }

        internal static void ChangeMotto(GameClientMessageHandler handler)
        {
            handler.ChangeMotto();
        }

        internal static void GetWardrobe(GameClientMessageHandler handler)
        {
            handler.GetWardrobe();
        }

        internal static void SaveWardrobe(GameClientMessageHandler handler)
        {
            handler.SaveWardrobe();
        }

        internal static void GetPetsInventory(GameClientMessageHandler handler)
        {
            handler.GetPetsInventory();
        }

        internal static void OpenQuests(GameClientMessageHandler handler)
        {
            handler.OpenQuests();
        }

        internal static void StartQuest(GameClientMessageHandler handler)
        {
            handler.StartQuest();
        }

        internal static void StopQuest(GameClientMessageHandler handler)
        {
            handler.StopQuest();
        }

        internal static void CheckNameChange(GameClientMessageHandler handler)
        {
            handler.CheckNameChange();
        }

        internal static void SaveNameChange(GameClientMessageHandler handler)
        {
            handler.SaveNameChange();
        }

        internal static void GetCurrentQuest(GameClientMessageHandler handler)
        {
            handler.GetCurrentQuest();
        }

        internal static void ChangeManiquiInMemory(GameClientMessageHandler handler)
        {
            handler.ChangeManiquiInMemory();
        }

        internal static void SaveManiquiTODB(GameClientMessageHandler handler)
        {
            handler.SaveManiquiTODB();
        }

        internal static void ApplyBackgroundChanges(GameClientMessageHandler handler)
        {
            handler.ApplyBackgroundChanges();
        }

        internal static void GenerateBuyGroupPage(GameClientMessageHandler handler)
        {
            handler.GenerateBuyGroupPage();
        }

        internal static void CreateGuildMessageComposer(GameClientMessageHandler handler)
        {
            handler.CreateGuildMessageComposer();
        }

        internal static void ActivateGroupOnRoom(GameClientMessageHandler handler)
        {
            handler.ActivateGroupOnRoom();
        }

        internal static void ClickOnGroupItem(GameClientMessageHandler handler)
        {
            handler.ClickOnGroupItem();
        }

        internal static void GestionarGrupo(GameClientMessageHandler handler)
        {
            handler.GestionarGrupo();
        }

        internal static void SendGroupColors(GameClientMessageHandler handler)
        {
            handler.SendGroupColors();
        }

        internal static void SaveGroupIdentity(GameClientMessageHandler handler)
        {
            handler.SaveGroupIdentity();
        }

        internal static void SaveGroupImage(GameClientMessageHandler handler)
        {
            handler.SaveGroupImage();
        }

        internal static void SaveGroupColours(GameClientMessageHandler handler)
        {
            handler.SaveGroupColours();
        }

        internal static void SaveGroupSettings(GameClientMessageHandler handler)
        {
            handler.SaveGroupSettings();
        }

        internal static void LookGroupMembers(GameClientMessageHandler handler)
        {
            handler.LookGroupMembers();
        }

        internal static void TryJoinToGroup(GameClientMessageHandler handler)
        {
            handler.TryJoinToGroup();
        }

        internal static void NotifToLeaveGroup(GameClientMessageHandler handler)
        {
            handler.NotifToLeaveGroup();
        }

        internal static void LeaveGroup(GameClientMessageHandler handler)
        {
            handler.LeaveGroup();
        }

        internal static void CancelPetition(GameClientMessageHandler handler)
        {
            handler.CancelPetition();
        }

        internal static void AcceptMember(GameClientMessageHandler handler)
        {
            handler.AcceptMember();
        }

        internal static void DeleteFavoriteGroup(GameClientMessageHandler handler)
        {
            handler.DeleteFavoriteGroup();
        }

        internal static void ChangeFavoriteGroup(GameClientMessageHandler handler)
        {
            handler.ChangeFavoriteGroup();
        }

        internal static void GiveAdminGroup(GameClientMessageHandler handler)
        {
            handler.GiveAdminGroup();
        }

        internal static void QuitAdminGroup(GameClientMessageHandler handler)
        {
            handler.QuitAdminGroup();
        }

        internal static void LoadGroupsOnCata(GameClientMessageHandler handler)
        {
            handler.LoadGroupsOnCata();
        }

        internal static void DeleteGroup(GameClientMessageHandler handler)
        {
            handler.DeleteGroup();
        }

        internal static void SerializeBotInventory(GameClientMessageHandler handler)
        {
            handler.GetBotsInventory();
        }

        internal static void AddBotToRoom(GameClientMessageHandler handler)
        {
            handler.AddBotToRoom();
        }

        internal static void RemoveBotFromRoom(GameClientMessageHandler handler)
        {
            handler.RemoveBotFromRoom();
        }

        internal static void LoadChangeName(GameClientMessageHandler handler)
        {
            handler.LoadChangeName();
        }

        internal static void ChangeBotName(GameClientMessageHandler handler)
        {
            handler.ChangeBotName();
        }

        internal static void SaveAdsMpu(GameClientMessageHandler handler)
        {
            handler.SaveAdsMpu();
        }

        internal static void AddRelation(GameClientMessageHandler handler)
        {
            handler.AddRelation();
        }

        internal static void SerializeRelation(GameClientMessageHandler handler)
        {
            handler.SerializeRelation();
        }

        internal static void SerializeNews(GameClientMessageHandler handler)
        {
            handler.LoadNews();
        }

        internal static void StartYoutubeVideo(GameClientMessageHandler handler)
        {
            handler.StartYoutubeVideo();
        }

        internal static void ChangeYoutubeVideo(GameClientMessageHandler handler)
        {
            handler.ChangeYoutubeVideo();
        }

        internal static void SaveVolumen(GameClientMessageHandler handler)
        {
            handler.SaveVolumen();
        }

        internal static void UpdateItemTileHeight(GameClientMessageHandler handler)
        {
            handler.UpdateItemTileHeight();
        }

        internal static void OpenFurniMaticPage(GameClientMessageHandler handler)
        {
            handler.OpenFurniMaticPage();
        }

        internal static void RecycleItem(GameClientMessageHandler handler)
        {
            handler.RecycleItem();
        }

        internal static void CreateGnomo(GameClientMessageHandler handler)
        {
            handler.CreateGnomo();
        }

        internal static void PreferOldChat(GameClientMessageHandler handler)
        {
            handler.PreferOldChat();
        }

        internal static void AddRoomToSelectionStaff(GameClientMessageHandler handler)
        {
            handler.AddRoomToSelectionStaff();
        }

        internal static void ReportarAcoso(GameClientMessageHandler handler)
        {
            handler.ReportarAcoso();
        }

        internal static void ReportarAcosoMessage(GameClientMessageHandler handler)
        {
            handler.ReportarAcosoMessage();
        }

        internal static void SaveNewModelMap(GameClientMessageHandler handler)
        {
            handler.SaveNewModelMap();
        }

        internal static void SearchCatalogItem(GameClientMessageHandler handler)
        {
            handler.SearchCatalogItem();
        }

        internal static void ReloadFloorCommand(GameClientMessageHandler handler)
        {
            handler.ReloadFloorCommand();
        }

        internal static void FilterRoomPanel(GameClientMessageHandler handler)
        {
            handler.FilterRoomPanel();
        }

        internal static void AddWordToFilterRoom(GameClientMessageHandler handler)
        {
            handler.AddWordToFilterRoom();
        }

        internal static void ViewGroupForum(GameClientMessageHandler handler)
        {
            handler.ViewGroupForum();
        }

        internal static void SaveForumSettings(GameClientMessageHandler handler)
        {
            handler.SaveForumSettings();
        }

        internal static void CreateForumPost(GameClientMessageHandler handler)
        {
            handler.CreateForumPost();
        }

        internal static void PostClosedAndDisabled(GameClientMessageHandler handler)
        {
            handler.PostClosedAndDisabled();
        }

        internal static void PostHidden(GameClientMessageHandler handler)
        {
            handler.PostHidden();
        }

        internal static void OpenPost(GameClientMessageHandler handler)
        {
            handler.OpenPost();
        }

        internal static void SubPostHidden(GameClientMessageHandler handler)
        {
            handler.SubPostHidden();
        }

        internal static void MyForums(GameClientMessageHandler handler)
        {
            handler.MyForums();
        }

        internal static void ReportForumPost(GameClientMessageHandler handler)
        {
            handler.ReportForumPost();
        }

        internal static void CancelPoll(GameClientMessageHandler handler)
        {
            handler.CancelPoll();
        }

        internal static void InitPoll(GameClientMessageHandler handler)
        {
            handler.InitPoll();
        }

        internal static void EndPoll(GameClientMessageHandler handler)
        {
            handler.EndPoll();
        }

        internal static void EnableCamera(GameClientMessageHandler handler)
        {
            handler.EnableCamera();
        }

        internal static void SaveFballClothes(GameClientMessageHandler handler)
        {
            handler.SaveFballClothes();
        }

        internal static void CancelPetBreeding(GameClientMessageHandler handler)
        {
            handler.CancelPetBreeding();
        }

        internal static void CreatePetBreeding(GameClientMessageHandler handler)
        {
            handler.CreatePetBreeding();
        }

        internal static void IgnoreRoomInvitation(GameClientMessageHandler handler)
        {
            handler.IgnoreRoomInvitation();
        }

        internal static void StartHabboQuiz(GameClientMessageHandler handler)
        {
            handler.StartHabboQuiz();
        }

        internal static void FinishHabboQuiz(GameClientMessageHandler handler)
        {
            handler.FinishHabboQuiz();
        }

        internal static void OpenTalents(GameClientMessageHandler handler)
        {
            handler.OpenTalents();
        }

        internal static void OpenHabboAlfa(GameClientMessageHandler handler)
        {
            handler.OpenHabboAlfa();
        }

        internal static void CallForAlfaHelp(GameClientMessageHandler handler)
        {
            handler.CallForAlfaHelp();
        }

        internal static void AcceptOrNotAlfaHelp(GameClientMessageHandler handler)
        {
            handler.AcceptOrNotAlfaHelp();
        }

        internal static void AlfaOpinion(GameClientMessageHandler handler)
        {
            handler.AlfaOpinion();
        }

        internal static void ExitAlfaHelpVotation(GameClientMessageHandler handler)
        {
            handler.ExitAlfaHelpVotation();
        }

        internal static void ResponseAlfaHelp(GameClientMessageHandler handler)
        {
            handler.ResponseAlfaHelp();
        }

        internal static void AlfaHelpChat(GameClientMessageHandler handler)
        {
            handler.AlfaHelpChat();
        }

        internal static void IsWrittingAlfa(GameClientMessageHandler handler)
        {
            handler.IsWrittingAlfa();
        }

        internal static void CloseAlfaLink(GameClientMessageHandler handler)
        {
            handler.CloseAlfaLink();
        }

        internal static void RecomendHelpers(GameClientMessageHandler handler)
        {
            handler.RecomendHelpers();
        }

        internal static void CancelAlfaHelp(GameClientMessageHandler handler)
        {
            handler.CancelAlfaHelp();
        }

        internal static void AlfaChatVisit(GameClientMessageHandler handler)
        {
            handler.AlfaChatVisit();
        }

        internal static void AlfaChatInvite(GameClientMessageHandler handler)
        {
            handler.AlfaChatInvite();
        }

        internal static void VerifyUsersLock(GameClientMessageHandler handler)
        {
            handler.VerifyUsersLock();
        }

        internal static void EnableNewNavigator(GameClientMessageHandler handler)
        {
            handler.EnableNewNavigator();
        }

        internal static void NewNavigatorPacket(GameClientMessageHandler handler)
        {
            handler.NewNavigatorPacket();
        }

        internal static void EnableFocusUser(GameClientMessageHandler handler)
        {
            handler.EnableFocusUser();
        }

        internal static void SaveNavigatorSearch(GameClientMessageHandler handler)
        {
            handler.SaveNavigatorSearch();
        }

        internal static void DeleteNavigatorSearch(GameClientMessageHandler handler)
        {
            handler.DeleteNavigatorSearch();
        }

        internal static void GoRandomRoom(GameClientMessageHandler handler)
        {
            handler.GoRandomRoom();
        }

        internal static void RenderRoomMessageComposer(GameClientMessageHandler handler)
        {
            handler.RenderRoomMessageComposer();
        }

        internal static void RenderRoomMessageComposerBigPhoto(GameClientMessageHandler handler)
        {
            handler.RenderRoomMessageComposerBigPhoto();
        }

        internal static void BuyServerCameraPhoto(GameClientMessageHandler handler)
        {
            handler.BuyServerCameraPhoto();
        }

        internal static void BuyTargetedOffer(GameClientMessageHandler handler)
        {
            handler.BuyTargetedOffer();
        }

        internal static void GetCraftableInfo(GameClientMessageHandler handler)
        {
            handler.GetCraftableInfo();
        }

        internal static void InitializeCraftable(GameClientMessageHandler handler)
        {
            handler.InitializeCraftable();
        }

        internal static void GetCraftingRecipesAvailableComposer(GameClientMessageHandler handler)
        {
            handler.GetCraftingRecipesAvailableComposer();
        }

        internal static void RefreshCraftingTable(GameClientMessageHandler handler)
        {
            handler.RefreshCraftingTable();
        }

        internal static void CraftSecretComposer(GameClientMessageHandler handler)
        {
            handler.CraftSecretComposer();
        }

        internal static void AlertaEmbajador(GameClientMessageHandler handler)
        {
            handler.AlertaEmbajador();
        }

        internal static void PlacePremiumFloorItem(GameClientMessageHandler handler)
        {
            handler.PlacePremiumItem(true);
        }

        internal static void PlacePremiumWallItem(GameClientMessageHandler handler)
        {
            handler.PlacePremiumItem(false);
        }

        internal static void PurchasableClothingConfirmation(GameClientMessageHandler handler)
        {
            handler.PurchasableClothingConfirmation();
        }

        internal static void ShowNewUserInformation(GameClientMessageHandler handler)
        {
            handler.ShowNewUserInformation();
        }
        internal static void SMSVerificar(GameClientMessageHandler handler)
        {
            handler.SMSVerificar();
        }
        internal static void GetNuxPresentEvent(GameClientMessageHandler handler)
        {
            handler.GetNuxPresentEvent();
        }

        internal static void NuxMsgseiLaOq(GameClientMessageHandler handler)
        {
            handler.NuxMsgseiLaOq();
        }    
    }
}
