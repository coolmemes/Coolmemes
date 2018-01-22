using System;
using System.Collections;
using HabboEvents;
using ButterStorm;
using System.Diagnostics;

namespace Butterfly.Messages.StaticMessageHandlers
{
    class StaticClientMessageHandler
    {
        private delegate void StaticRequestHandler(GameClientMessageHandler handler);
        private static StaticRequestHandler[] handlers;

        private const int MAX_PACKET_HEADER = 0xFFF;

        internal static void Initialize()
        {
            handlers = new StaticRequestHandler[MAX_PACKET_HEADER];
            PacketsCollapsed.Initialize();
            RegisterPacketLibary();
        }

        internal static void HandlePacket(GameClientMessageHandler handler, ClientMessage message)
        {
            if (message.Id < 0 || message.Id >= MAX_PACKET_HEADER)
                return;

            if (!handler.GetPacketsUserLogs().CanReceivePacket(message.Id))
                return;

            if (handlers[message.Id] != null)
            {
                if (EmuSettings.SHOW_PACKETS)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(@" » Handling ID: " + message.ToString());
                    Console.ResetColor();
                }
                handlers[message.Id].Invoke(handler);
            }
            else if (EmuSettings.SHOW_PACKETS)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" » Unknown packet ID: " + message.ToString());
                Console.ResetColor();
            }
        }

        #region Register
        internal static void RegisterPacketLibary()
        {
            handlers[(int)Incoming.CheckReleaseMessageEvent] = new StaticRequestHandler(SharedPacketLib.CheckHabboRelease);
            handlers[(int)Incoming.InitCrypto] = new StaticRequestHandler(SharedPacketLib.InitCrypto);
            handlers[(int)Incoming.SecretKey] = new StaticRequestHandler(SharedPacketLib.SecretKey);
            handlers[(int)Incoming.ClientVars] = new StaticRequestHandler(SharedPacketLib.setVars);
            handlers[(int)Incoming.UniqueMachineID] = new StaticRequestHandler(SharedPacketLib.setUniqueId);
            handlers[(int)Incoming.ParseCampaingData] = new StaticRequestHandler(SharedPacketLib.SendCampaingData);
            handlers[(int)Incoming.OpenCatalog] = new StaticRequestHandler(SharedPacketLib.GetCatalogIndex);
            handlers[(int)Incoming.OpenCatalogPage] = new StaticRequestHandler(SharedPacketLib.GetCatalogPage);
            handlers[(int)Incoming.RedeemVoucher] = new StaticRequestHandler(SharedPacketLib.RedeemVoucher);
            handlers[(int)Incoming.PurchaseCatalogItem] = new StaticRequestHandler(SharedPacketLib.HandlePurchase);
            handlers[(int)Incoming.PurchaseGift] = new StaticRequestHandler(SharedPacketLib.PurchaseGift);
            handlers[(int)Incoming.SecondaryGoToRoom] = new StaticRequestHandler(SharedPacketLib.enterOnRoom2);
            handlers[(int)Incoming.CatalogData1] = new StaticRequestHandler(SharedPacketLib.GetCataData1);
            handlers[(int)Incoming.CheckPetName] = new StaticRequestHandler(SharedPacketLib.CheckPetName);
            handlers[(int)Incoming.CatalogGetRace] = new StaticRequestHandler(SharedPacketLib.PetRaces);
            handlers[(int)Incoming.AppliSitDown] = new StaticRequestHandler(SharedPacketLib.SitDown);
            handlers[(int)Incoming.SSOTicket] = new StaticRequestHandler(SharedPacketLib.SSOLogin);
            handlers[(int)Incoming.Tags] = new StaticRequestHandler(SharedPacketLib.attTags);
            handlers[(int)Incoming.RequestHelp] = new StaticRequestHandler(SharedPacketLib.RequestHelp);
            handlers[(int)Incoming.CreateTicket] = new StaticRequestHandler(SharedPacketLib.SubmitHelpTicket);
            handlers[(int)Incoming.SendRoomAlert] = new StaticRequestHandler(SharedPacketLib.ModSendRoomAlert);
            handlers[(int)Incoming.PickIssue] = new StaticRequestHandler(SharedPacketLib.ModPickTicket);
            handlers[(int)Incoming.ReleaseIssue] = new StaticRequestHandler(SharedPacketLib.ModReleaseTicket);
            handlers[(int)Incoming.CloseIssue] = new StaticRequestHandler(SharedPacketLib.ModCloseTicket);
            handlers[(int)Incoming.ToolForUser] = new StaticRequestHandler(SharedPacketLib.ModGetUserInfo);
            handlers[(int)Incoming.UserChatlog] = new StaticRequestHandler(SharedPacketLib.ModGetUserChatlog);
            handlers[(int)Incoming.OpenRoomChatlog] = new StaticRequestHandler(SharedPacketLib.ModGetRoomChatlog);
            handlers[(int)Incoming.IssueChatlog] = new StaticRequestHandler(SharedPacketLib.ModGetTicketChatlog);
            // handlers[(int)Incoming.GetRoomVisits] = new StaticRequestHandler(SharedPacketLib.ModGetRoomVisits);
            handlers[(int)Incoming.ToolForThisRoom] = new StaticRequestHandler(SharedPacketLib.ModGetRoomTool);
            handlers[(int)Incoming.PerformRoomAction] = new StaticRequestHandler(SharedPacketLib.ModPerformRoomAction);
            handlers[(int)Incoming.SendMessageByTemplate] = new StaticRequestHandler(SharedPacketLib.ModSendUserMessage);
            handlers[(int)Incoming.ModActionKickUser] = new StaticRequestHandler(SharedPacketLib.ModKickUser);
            handlers[(int)Incoming.ModActionMuteUser] = new StaticRequestHandler(SharedPacketLib.ModMuteUser);
            handlers[(int)Incoming.ModActionBanUser] = new StaticRequestHandler(SharedPacketLib.ModBanUser);
            handlers[(int)Incoming.DeleteFriend] = new StaticRequestHandler(SharedPacketLib.RemoveBuddy);
            handlers[(int)Incoming.SearchFriend] = new StaticRequestHandler(SharedPacketLib.SearchHabbo);
            handlers[(int)Incoming.SendInstantMessenger] = new StaticRequestHandler(SharedPacketLib.SendInstantMessenger);
            handlers[(int)Incoming.AcceptRequest] = new StaticRequestHandler(SharedPacketLib.AcceptRequest);
            handlers[(int)Incoming.DeclineRequest] = new StaticRequestHandler(SharedPacketLib.DeclineRequest);
            handlers[(int)Incoming.FriendRequest] = new StaticRequestHandler(SharedPacketLib.RequestBuddy);
            handlers[(int)Incoming.FollowFriend] = new StaticRequestHandler(SharedPacketLib.FollowBuddy);
            handlers[(int)Incoming.InviteFriendsToMyRoom] = new StaticRequestHandler(SharedPacketLib.SendInstantInvite);
            handlers[(int)Incoming.AddFavourite] = new StaticRequestHandler(SharedPacketLib.AddFavorite);
            handlers[(int)Incoming.RemoveFavourite] = new StaticRequestHandler(SharedPacketLib.RemoveFavorite);
            handlers[(int)Incoming.GoToHotelView] = new StaticRequestHandler(SharedPacketLib.GoToHotelView);
            handlers[(int)Incoming.PromotedGetMyRooms] = new StaticRequestHandler(SharedPacketLib.PromotedGetMyRooms);
            handlers[(int)Incoming.StartEvent] = new StaticRequestHandler(SharedPacketLib.StartEvent);
            handlers[(int)Incoming.EditEvent] = new StaticRequestHandler(SharedPacketLib.EditEvent);
            handlers[(int)Incoming.LoadCategorys] = new StaticRequestHandler(SharedPacketLib.GetFlatCats);
            handlers[(int)Incoming.LoadFeaturedRooms] = new StaticRequestHandler(SharedPacketLib.NewNavigatorPacket);
            handlers[(int)Incoming.LoadHeightMap] = new StaticRequestHandler(SharedPacketLib.GetRoomData2);
            handlers[(int)Incoming.AddUserToRoomFirstTime] = new StaticRequestHandler(SharedPacketLib.GetRoomData3);
            handlers[(int)Incoming.AddUserToRoom] = new StaticRequestHandler(SharedPacketLib.GetRoomData3);
            handlers[(int)Incoming.Talk] = new StaticRequestHandler(SharedPacketLib.Talk);
            handlers[(int)Incoming.Shout] = new StaticRequestHandler(SharedPacketLib.Shout);
            handlers[(int)Incoming.Whisp] = new StaticRequestHandler(SharedPacketLib.Whisper);
            handlers[(int)Incoming.Move] = new StaticRequestHandler(SharedPacketLib.Move);
            handlers[(int)Incoming.CanCreateRoom] = new StaticRequestHandler(SharedPacketLib.CanCreateRoom);
            handlers[(int)Incoming.CreateRoom] = new StaticRequestHandler(SharedPacketLib.CreateRoom);
            handlers[(int)Incoming.LoadFirstRoomData] = new StaticRequestHandler(SharedPacketLib.enterOnRoom);
            handlers[(int)Incoming.GetRoomData] = new StaticRequestHandler(SharedPacketLib.GetRoomEditData);
            handlers[(int)Incoming.SaveRoomData] = new StaticRequestHandler(SharedPacketLib.SaveRoomData);
            handlers[(int)Incoming.ReloadRights] = new StaticRequestHandler(SharedPacketLib.RelaodRights);
            handlers[(int)Incoming.GiveRights] = new StaticRequestHandler(SharedPacketLib.GiveRights);
            handlers[(int)Incoming.RemoveRightsMessageComposer] = new StaticRequestHandler(SharedPacketLib.TakeRights);
            handlers[(int)Incoming.RemoveAllRights] = new StaticRequestHandler(SharedPacketLib.TakeAllRights);
            handlers[(int)Incoming.KickUserOfRoom] = new StaticRequestHandler(SharedPacketLib.KickUser);
            handlers[(int)Incoming.BanUserOfRoom] = new StaticRequestHandler(SharedPacketLib.BanUser);
            handlers[(int)Incoming.MuteUser] = new StaticRequestHandler(SharedPacketLib.MuteUser);
            handlers[(int)Incoming.MuteRoom] = new StaticRequestHandler(SharedPacketLib.MuteRoom);
            handlers[(int)Incoming.ReloadBans] = new StaticRequestHandler(SharedPacketLib.ReloadBans);
            handlers[(int)Incoming.RemoveBans] = new StaticRequestHandler(SharedPacketLib.RemoveBans);
            handlers[(int)Incoming.StartTrade] = new StaticRequestHandler(SharedPacketLib.InitTrade);
            handlers[(int)Incoming.SetHome] = new StaticRequestHandler(SharedPacketLib.SetHomeRoom);
            handlers[(int)Incoming.RemoveRoom] = new StaticRequestHandler(SharedPacketLib.DeleteRoom);
            handlers[(int)Incoming.LookTo] = new StaticRequestHandler(SharedPacketLib.LookAt);
            handlers[(int)Incoming.StartTyping] = new StaticRequestHandler(SharedPacketLib.StartTyping);
            handlers[(int)Incoming.StopTyping] = new StaticRequestHandler(SharedPacketLib.StopTyping);
            handlers[(int)Incoming.IgnoreUser] = new StaticRequestHandler(SharedPacketLib.IgnoreUser);
            handlers[(int)Incoming.UnignoreUser] = new StaticRequestHandler(SharedPacketLib.UnignoreUser);
            handlers[(int)Incoming.ApplyAction] = new StaticRequestHandler(SharedPacketLib.Wave);
            handlers[(int)Incoming.ApplySign] = new StaticRequestHandler(SharedPacketLib.Sign);
            // handlers[(int)Incoming.GetUserTags] = new StaticRequestHandler(SharedPacketLib.GetUserTags);
            handlers[(int)Incoming.GetUserBadges] = new StaticRequestHandler(SharedPacketLib.GetUserBadges);
            handlers[(int)Incoming.GiveRoomScore] = new StaticRequestHandler(SharedPacketLib.RateRoom);
            handlers[(int)Incoming.ApplyDance] = new StaticRequestHandler(SharedPacketLib.Dance);
            handlers[(int)Incoming.RemoveHanditem] = new StaticRequestHandler(SharedPacketLib.RemoveHanditem);
            handlers[(int)Incoming.GiveObject] = new StaticRequestHandler(SharedPacketLib.GiveHanditem);
            handlers[(int)Incoming.AnswerDoorBell] = new StaticRequestHandler(SharedPacketLib.AnswerDoorbell);
            handlers[(int)Incoming.ReqLoadByDoorBell] = new StaticRequestHandler(SharedPacketLib.ReqLoadRoomForUser);
            handlers[(int)Incoming.ApplySpace] = new StaticRequestHandler(SharedPacketLib.ApplyRoomEffect);
            handlers[(int)Incoming.AddFloorItem] = new StaticRequestHandler(SharedPacketLib.PlaceItem);
            handlers[(int)Incoming.PickupItem] = new StaticRequestHandler(SharedPacketLib.TakeItem);
            handlers[(int)Incoming.MoveOrRotate] = new StaticRequestHandler(SharedPacketLib.MoveItem);
            handlers[(int)Incoming.MoveWall] = new StaticRequestHandler(SharedPacketLib.MoveWallItem);
            handlers[(int)Incoming.HandleItem] = new StaticRequestHandler(SharedPacketLib.TriggerItem);
            handlers[(int)Incoming.OpenDice] = new StaticRequestHandler(SharedPacketLib.TriggerItem); // To dices
            handlers[(int)Incoming.RunDice] = new StaticRequestHandler(SharedPacketLib.TriggerItemDiceSpecial);  // To dices
            handlers[(int)Incoming.OpenOneWayGate] = new StaticRequestHandler(SharedPacketLib.TriggerItem);
            handlers[(int)Incoming.HandleWallItem] = new StaticRequestHandler(SharedPacketLib.TriggerItem);
            handlers[(int)Incoming.HandleWallItem2] = new StaticRequestHandler(SharedPacketLib.TriggerItem);
            handlers[(int)Incoming.HandleWheel] = new StaticRequestHandler(SharedPacketLib.TriggerItem);
            handlers[(int)Incoming.OpenPostIt] = new StaticRequestHandler(SharedPacketLib.OpenPostit);
            handlers[(int)Incoming.SavePostIt] = new StaticRequestHandler(SharedPacketLib.SavePostit);
            handlers[(int)Incoming.DeletePostIt] = new StaticRequestHandler(SharedPacketLib.DeletePostit);
            handlers[(int)Incoming.OpenGift] = new StaticRequestHandler(SharedPacketLib.OpenPresent);
            handlers[(int)Incoming.StartMoodlight] = new StaticRequestHandler(SharedPacketLib.GetMoodlight);
            handlers[(int)Incoming.ApplyMoodlightChanges] = new StaticRequestHandler(SharedPacketLib.UpdateMoodlight);
            handlers[(int)Incoming.TurnOnMoodlight] = new StaticRequestHandler(SharedPacketLib.SwitchMoodlightStatus);
            handlers[(int)Incoming.SendOffer] = new StaticRequestHandler(SharedPacketLib.OfferTradeItem);
            handlers[(int)Incoming.SendMultiOffer] = new StaticRequestHandler(SharedPacketLib.OfferTradeMultiItem);
            handlers[(int)Incoming.CancelOffer] = new StaticRequestHandler(SharedPacketLib.TakeBackTradeItem);
            handlers[(int)Incoming.CancelTrade] = new StaticRequestHandler(SharedPacketLib.StopTrade);
            handlers[(int)Incoming.CancelTrade2] = new StaticRequestHandler(SharedPacketLib.StopTrade);
            handlers[(int)Incoming.AcceptTrade] = new StaticRequestHandler(SharedPacketLib.AcceptTrade);
            handlers[(int)Incoming.UnacceptTrade] = new StaticRequestHandler(SharedPacketLib.UnacceptTrade);
            handlers[(int)Incoming.ConfirmTrade] = new StaticRequestHandler(SharedPacketLib.CompleteTrade);
            handlers[(int)Incoming.SendRespects] = new StaticRequestHandler(SharedPacketLib.GiveRespect);
            handlers[(int)Incoming.StartEffect] = new StaticRequestHandler(SharedPacketLib.ApplyEffect);
            handlers[(int)Incoming.EnableEffect] = new StaticRequestHandler(SharedPacketLib.EnableEffect);
            handlers[(int)Incoming.RedeemExchangeFurni] = new StaticRequestHandler(SharedPacketLib.RedeemExchangeFurni);
            handlers[(int)Incoming.PlacePet] = new StaticRequestHandler(SharedPacketLib.PlacePet);
            handlers[(int)Incoming.PetInfo] = new StaticRequestHandler(SharedPacketLib.GetPetInfo);
            handlers[(int)Incoming.PickupPet] = new StaticRequestHandler(SharedPacketLib.PickUpPet);
            handlers[(int)Incoming.RespetPet] = new StaticRequestHandler(SharedPacketLib.RespectPet);
            handlers[(int)Incoming.AddPostIt] = new StaticRequestHandler(SharedPacketLib.PlacePostIt);
            handlers[(int)Incoming.AddSaddleToPet] = new StaticRequestHandler(SharedPacketLib.AddSaddle);
            handlers[(int)Incoming.RemoveSaddle] = new StaticRequestHandler(SharedPacketLib.RemoveSaddle);
            handlers[(int)Incoming.MountOnPet] = new StaticRequestHandler(SharedPacketLib.Ride);
            handlers[(int)Incoming.AllCanMount] = new StaticRequestHandler(SharedPacketLib.AllCanMount);
            handlers[(int)Incoming.SaveWiredEffect] = new StaticRequestHandler(SharedPacketLib.SaveWired);
            handlers[(int)Incoming.SaveWiredTrigger] = new StaticRequestHandler(SharedPacketLib.SaveWired);
            handlers[(int)Incoming.SaveWiredCondition] = new StaticRequestHandler(SharedPacketLib.SaveWiredCondition);
            handlers[(int)Incoming.LoadProfile] = new StaticRequestHandler(SharedPacketLib.LoadProfile);
            handlers[(int)Incoming.BadgesInventary] = new StaticRequestHandler(SharedPacketLib.GetBadges);
            handlers[(int)Incoming.ApplyBadge] = new StaticRequestHandler(SharedPacketLib.UpdateBadges);
            handlers[(int)Incoming.OpenAchievements] = new StaticRequestHandler(SharedPacketLib.GetAchievements);
            handlers[(int)Incoming.ChangeLook] = new StaticRequestHandler(SharedPacketLib.ChangeLook);
            handlers[(int)Incoming.ChangeMotto] = new StaticRequestHandler(SharedPacketLib.ChangeMotto);
            handlers[(int)Incoming.GetWardrobe] = new StaticRequestHandler(SharedPacketLib.GetWardrobe);
            handlers[(int)Incoming.SaveWardrobe] = new StaticRequestHandler(SharedPacketLib.SaveWardrobe);
            handlers[(int)Incoming.OpenInventary] = new StaticRequestHandler(SharedPacketLib.GetInventory);
            handlers[(int)Incoming.PetInventary] = new StaticRequestHandler(SharedPacketLib.GetPetsInventory);
            handlers[(int)Incoming.OpenQuests] = new StaticRequestHandler(SharedPacketLib.OpenQuests);
            handlers[(int)Incoming.ActiveQuests] = new StaticRequestHandler(SharedPacketLib.StartQuest);
            handlers[(int)Incoming.CancelQuests] = new StaticRequestHandler(SharedPacketLib.StopQuest);
            handlers[(int)Incoming.ActiveEndedQuest] = new StaticRequestHandler(SharedPacketLib.GetCurrentQuest);
            handlers[(int)Incoming.CheckNameChange] = new StaticRequestHandler(SharedPacketLib.CheckNameChange);
            handlers[(int)Incoming.SaveNameChange] = new StaticRequestHandler(SharedPacketLib.SaveNameChange);
            handlers[(int)Incoming.MarketplaceCanSell] = new StaticRequestHandler(SharedPacketLib.MarketplaceCanSell);
            handlers[(int)Incoming.MarketplaceSetPrice] = new StaticRequestHandler(SharedPacketLib.MarketplaceSetPrice);
            handlers[(int)Incoming.MarketplacePostItem] = new StaticRequestHandler(SharedPacketLib.MarketplacePostItem);
            handlers[(int)Incoming.MarketplaceGetOwnOffers] = new StaticRequestHandler(SharedPacketLib.MarketplaceGetOwnOffers);
            handlers[(int)Incoming.MarketplaceTakeBack] = new StaticRequestHandler(SharedPacketLib.MarketplaceTakeBack);
            handlers[(int)Incoming.MarketplaceClaimCredits] = new StaticRequestHandler(SharedPacketLib.MarketplaceClaimCredits);
            handlers[(int)Incoming.MarketplaceGetOffers] = new StaticRequestHandler(SharedPacketLib.MarketplaceGetOffers);
            handlers[(int)Incoming.MarketplacePurchase] = new StaticRequestHandler(SharedPacketLib.MarketplacePurchase);
            handlers[(int)Incoming.ListenPreview] = new StaticRequestHandler(SharedPacketLib.GetMusicData);
            handlers[(int)Incoming.LoadInvSongs] = new StaticRequestHandler(SharedPacketLib.LoadInvSongs);
            handlers[(int)Incoming.LoadJukeSongs] = new StaticRequestHandler(SharedPacketLib.LoadJukeSongs);
            handlers[(int)Incoming.AddNewCdToJuke] = new StaticRequestHandler(SharedPacketLib.AddNewCdToJuke);
            handlers[(int)Incoming.RemoveCdToJuke] = new StaticRequestHandler(SharedPacketLib.RemoveCdToJuke);
            handlers[(int)Incoming.ChangeManiquiInMemory] = new StaticRequestHandler(SharedPacketLib.ChangeManiquiInMemory);
            handlers[(int)Incoming.SaveManiquiTODB] = new StaticRequestHandler(SharedPacketLib.SaveManiquiTODB);
            handlers[(int)Incoming.ApplyBackgroundChanges] = new StaticRequestHandler(SharedPacketLib.ApplyBackgroundChanges);
            handlers[(int)Incoming.GenerateBuyGroupPage] = new StaticRequestHandler(SharedPacketLib.GenerateBuyGroupPage);
            handlers[(int)Incoming.CreateGuildMessageComposer] = new StaticRequestHandler(SharedPacketLib.CreateGuildMessageComposer);
            handlers[(int)Incoming.ActivateGroupOnRoom] = new StaticRequestHandler(SharedPacketLib.ActivateGroupOnRoom);
            handlers[(int)Incoming.ClickOnGroupItem] = new StaticRequestHandler(SharedPacketLib.ClickOnGroupItem);
            handlers[(int)Incoming.GestionarGrupo] = new StaticRequestHandler(SharedPacketLib.GestionarGrupo);
            handlers[(int)Incoming.SendGroupColors] = new StaticRequestHandler(SharedPacketLib.SendGroupColors);
            handlers[(int)Incoming.SaveGroupIdentity] = new StaticRequestHandler(SharedPacketLib.SaveGroupIdentity);
            handlers[(int)Incoming.SaveGroupImage] = new StaticRequestHandler(SharedPacketLib.SaveGroupImage);
            handlers[(int)Incoming.SaveGroupColours] = new StaticRequestHandler(SharedPacketLib.SaveGroupColours);
            handlers[(int)Incoming.SaveGroupSettings] = new StaticRequestHandler(SharedPacketLib.SaveGroupSettings);
            handlers[(int)Incoming.LookGroupMembers] = new StaticRequestHandler(SharedPacketLib.LookGroupMembers);
            handlers[(int)Incoming.TryJoinToGroup] = new StaticRequestHandler(SharedPacketLib.TryJoinToGroup);
            handlers[(int)Incoming.NotifToLeaveGroup] = new StaticRequestHandler(SharedPacketLib.NotifToLeaveGroup);
            handlers[(int)Incoming.LeaveGroup] = new StaticRequestHandler(SharedPacketLib.LeaveGroup);
            handlers[(int)Incoming.CancelPetition] = new StaticRequestHandler(SharedPacketLib.CancelPetition);
            handlers[(int)Incoming.AcceptMember] = new StaticRequestHandler(SharedPacketLib.AcceptMember);
            handlers[(int)Incoming.DeleteFavoriteGroup] = new StaticRequestHandler(SharedPacketLib.DeleteFavoriteGroup);
            handlers[(int)Incoming.ChangeFavoriteGroup] = new StaticRequestHandler(SharedPacketLib.ChangeFavoriteGroup);
            handlers[(int)Incoming.GiveAdminGroup] = new StaticRequestHandler(SharedPacketLib.GiveAdminGroup);
            handlers[(int)Incoming.QuitAdminGroup] = new StaticRequestHandler(SharedPacketLib.QuitAdminGroup);
            handlers[(int)Incoming.LoadGroupsOnCata] = new StaticRequestHandler(SharedPacketLib.LoadGroupsOnCata);
            handlers[(int)Incoming.DeleteGroup] = new StaticRequestHandler(SharedPacketLib.DeleteGroup);
            handlers[(int)Incoming.SerializeBotInventory] = new StaticRequestHandler(SharedPacketLib.SerializeBotInventory);
            handlers[(int)Incoming.AddBotToRoom] = new StaticRequestHandler(SharedPacketLib.AddBotToRoom);
            handlers[(int)Incoming.RemoveBotFromRoom] = new StaticRequestHandler(SharedPacketLib.RemoveBotFromRoom);
            handlers[(int)Incoming.LoadChangeName] = new StaticRequestHandler(SharedPacketLib.LoadChangeName);
            handlers[(int)Incoming.ChangeBotName] = new StaticRequestHandler(SharedPacketLib.ChangeBotName);
            handlers[(int)Incoming.SaveAdsMpu] = new StaticRequestHandler(SharedPacketLib.SaveAdsMpu);
            handlers[(int)Incoming.AddRelation] = new StaticRequestHandler(SharedPacketLib.AddRelation);
            handlers[(int)Incoming.SerializeRelation] = new StaticRequestHandler(SharedPacketLib.SerializeRelation);
            handlers[(int)Incoming.RefreshNews] = new StaticRequestHandler(SharedPacketLib.SerializeNews);
            handlers[(int)Incoming.StartYoutubeVideo] = new StaticRequestHandler(SharedPacketLib.StartYoutubeVideo);
            handlers[(int)Incoming.ChangeYoutubeVideo] = new StaticRequestHandler(SharedPacketLib.ChangeYoutubeVideo);
            handlers[(int)Incoming.SaveVolumen] = new StaticRequestHandler(SharedPacketLib.SaveVolumen);
            handlers[(int)Incoming.UpdateItemTileHeight] = new StaticRequestHandler(SharedPacketLib.UpdateItemTileHeight);
            handlers[(int)Incoming.OpenFurniMaticPage] = new StaticRequestHandler(SharedPacketLib.OpenFurniMaticPage);
            handlers[(int)Incoming.RecycleItem] = new StaticRequestHandler(SharedPacketLib.RecycleItem);
            handlers[(int)Incoming.CreateGnomo] = new StaticRequestHandler(SharedPacketLib.CreateGnomo);
            handlers[(int)Incoming.PreferOldChat] = new StaticRequestHandler(SharedPacketLib.PreferOldChat);
            handlers[(int)Incoming.AddRoomToSelectionStaff] = new StaticRequestHandler(SharedPacketLib.AddRoomToSelectionStaff);
            handlers[(int)Incoming.ReportAcoso] = new StaticRequestHandler(SharedPacketLib.ReportarAcoso);
            handlers[(int)Incoming.ReportarAcosoMessage] = new StaticRequestHandler(SharedPacketLib.ReportarAcosoMessage);
            handlers[(int)Incoming.SaveNewModelMap] = new StaticRequestHandler(SharedPacketLib.SaveNewModelMap);
            handlers[(int)Incoming.SearchCatalogItem] = new StaticRequestHandler(SharedPacketLib.SearchCatalogItem);
            handlers[(int)Incoming.ReloadFloorCommand] = new StaticRequestHandler(SharedPacketLib.ReloadFloorCommand);
            handlers[(int)Incoming.ModToolsRoomsVisits] = new StaticRequestHandler(SharedPacketLib.ModGetRoomVisits);
            handlers[(int)Incoming.FilterRoomPanel] = new StaticRequestHandler(SharedPacketLib.FilterRoomPanel);
            handlers[(int)Incoming.AddWordToFilterRoom] = new StaticRequestHandler(SharedPacketLib.AddWordToFilterRoom);
            handlers[(int)Incoming.ViewGroupForum] = new StaticRequestHandler(SharedPacketLib.ViewGroupForum);
            handlers[(int)Incoming.SaveForumSettings] = new StaticRequestHandler(SharedPacketLib.SaveForumSettings);
            handlers[(int)Incoming.CreateForumPost] = new StaticRequestHandler(SharedPacketLib.CreateForumPost);
            handlers[(int)Incoming.UpdateThreadMessageComposer] = new StaticRequestHandler(SharedPacketLib.PostClosedAndDisabled);
            handlers[(int)Incoming.PostHidden] = new StaticRequestHandler(SharedPacketLib.PostHidden);
            handlers[(int)Incoming.OpenPost] = new StaticRequestHandler(SharedPacketLib.OpenPost);
            handlers[(int)Incoming.SubPostHidden] = new StaticRequestHandler(SharedPacketLib.SubPostHidden);
            handlers[(int)Incoming.MyForums] = new StaticRequestHandler(SharedPacketLib.MyForums);
            handlers[(int)Incoming.ReportForumPost] = new StaticRequestHandler(SharedPacketLib.ReportForumPost);
            handlers[(int)Incoming.CancelPoll] = new StaticRequestHandler(SharedPacketLib.CancelPoll);
            handlers[(int)Incoming.InitPoll] = new StaticRequestHandler(SharedPacketLib.InitPoll);
            handlers[(int)Incoming.PollAnswerComposer] = new StaticRequestHandler(SharedPacketLib.EndPoll);
            handlers[(int)Incoming.EnableCamera] = new StaticRequestHandler(SharedPacketLib.EnableCamera);
            handlers[(int)Incoming.PetCommands] = new StaticRequestHandler(SharedPacketLib.CommandsPet);
            handlers[(int)Incoming.SaveFballClothes] = new StaticRequestHandler(SharedPacketLib.SaveFballClothes);
            handlers[(int)Incoming.CancelPetBreeding] = new StaticRequestHandler(SharedPacketLib.CancelPetBreeding);
            handlers[(int)Incoming.CreatePetBreeding] = new StaticRequestHandler(SharedPacketLib.CreatePetBreeding);
            handlers[(int)Incoming.IgnoreRoomInvitation] = new StaticRequestHandler(SharedPacketLib.IgnoreRoomInvitation);
            handlers[(int)Incoming.StartHabboQuiz] = new StaticRequestHandler(SharedPacketLib.StartHabboQuiz);
            handlers[(int)Incoming.FinishHabboQuiz] = new StaticRequestHandler(SharedPacketLib.FinishHabboQuiz);
            handlers[(int)Incoming.OpenTalents] = new StaticRequestHandler(SharedPacketLib.OpenTalents);
            handlers[(int)Incoming.OpenHabboAlfa] = new StaticRequestHandler(SharedPacketLib.OpenHabboAlfa);
            handlers[(int)Incoming.CallForAlfaHelp] = new StaticRequestHandler(SharedPacketLib.CallForAlfaHelp);
            handlers[(int)Incoming.AcceptOrNotAlfaHelp] = new StaticRequestHandler(SharedPacketLib.AcceptOrNotAlfaHelp);
            handlers[(int)Incoming.AlfaOpinion] = new StaticRequestHandler(SharedPacketLib.AlfaOpinion);
            // handlers[(int)Incoming.ExitAlfaHelpVotation] = new StaticRequestHandler(SharedPacketLib.ExitAlfaHelpVotation);
            handlers[(int)Incoming.ResponseAlfaHelp] = new StaticRequestHandler(SharedPacketLib.ResponseAlfaHelp);
            handlers[(int)Incoming.AlfaHelpChat] = new StaticRequestHandler(SharedPacketLib.AlfaHelpChat);
            handlers[(int)Incoming.IsWrittingAlfa] = new StaticRequestHandler(SharedPacketLib.IsWrittingAlfa);
            handlers[(int)Incoming.CloseAlfaLink] = new StaticRequestHandler(SharedPacketLib.CloseAlfaLink);
            handlers[(int)Incoming.RecomendHelpers] = new StaticRequestHandler(SharedPacketLib.RecomendHelpers);
            handlers[(int)Incoming.CancelAlfaHelp] = new StaticRequestHandler(SharedPacketLib.CancelAlfaHelp);
            handlers[(int)Incoming.AlfaChatVisit] = new StaticRequestHandler(SharedPacketLib.AlfaChatVisit);
            handlers[(int)Incoming.AlfaChatInvite] = new StaticRequestHandler(SharedPacketLib.AlfaChatInvite);
            handlers[(int)Incoming.VerifyUsersLock] = new StaticRequestHandler(SharedPacketLib.VerifyUsersLock);
            handlers[(int)Incoming.EnableNewNavigator] = new StaticRequestHandler(SharedPacketLib.EnableNewNavigator);
            handlers[(int)Incoming.NewNavigatorPacket] = new StaticRequestHandler(SharedPacketLib.NewNavigatorPacket);
            handlers[(int)Incoming.EnableFocusUser] = new StaticRequestHandler(SharedPacketLib.EnableFocusUser);
            handlers[(int)Incoming.SaveNavigatorSearch] = new StaticRequestHandler(SharedPacketLib.SaveNavigatorSearch);
            handlers[(int)Incoming.DeleteNavigatorSearch] = new StaticRequestHandler(SharedPacketLib.DeleteNavigatorSearch);
            handlers[(int)Incoming.GoRandomRoom] = new StaticRequestHandler(SharedPacketLib.GoRandomRoom);
            handlers[(int)Incoming.RenderRoomMessageComposer] = new StaticRequestHandler(SharedPacketLib.RenderRoomMessageComposer);
            handlers[(int)Incoming.RenderRoomMessageComposerBigPhoto] = new StaticRequestHandler(SharedPacketLib.RenderRoomMessageComposerBigPhoto);
            handlers[(int)Incoming.BuyServerCameraPhoto] = new StaticRequestHandler(SharedPacketLib.BuyServerCameraPhoto);
            handlers[(int)Incoming.BuyTargetedOffer] = new StaticRequestHandler(SharedPacketLib.BuyTargetedOffer);
            handlers[(int)Incoming.GetCraftableInfo] = new StaticRequestHandler(SharedPacketLib.GetCraftableInfo);
            handlers[(int)Incoming.InitializeCraftable] = new StaticRequestHandler(SharedPacketLib.InitializeCraftable);
            handlers[(int)Incoming.GetCraftingRecipesAvailableComposer] = new StaticRequestHandler(SharedPacketLib.GetCraftingRecipesAvailableComposer);
            handlers[(int)Incoming.RefreshCraftingTable] = new StaticRequestHandler(SharedPacketLib.RefreshCraftingTable);
            handlers[(int)Incoming.CraftSecretComposer] = new StaticRequestHandler(SharedPacketLib.CraftSecretComposer);
            handlers[(int)Incoming.AlertaEmbajador] = new StaticRequestHandler(SharedPacketLib.AlertaEmbajador);
            handlers[(int)Incoming.PlacePremiumFloorItem] = new StaticRequestHandler(SharedPacketLib.PlacePremiumFloorItem);
            handlers[(int)Incoming.PlacePremiumWallItem] = new StaticRequestHandler(SharedPacketLib.PlacePremiumWallItem);
            handlers[(int)Incoming.PurchasableClothingConfirmation] = new StaticRequestHandler(SharedPacketLib.PurchasableClothingConfirmation);
            handlers[(int)Incoming.ShowNewUserInformation] = new StaticRequestHandler(SharedPacketLib.ShowNewUserInformation);
            handlers[(int)Incoming.SMSVerificar] = new StaticRequestHandler(SharedPacketLib.SMSVerificar);
            handlers[(int)Incoming.SMSVerificarBotaoDeBaixo] = new StaticRequestHandler(SharedPacketLib.SMSVerificar);
            handlers[(int)Incoming.GetNuxPresentEvent] = new StaticRequestHandler(SharedPacketLib.GetNuxPresentEvent);
            handlers[(int)Incoming.NuxMsgseiLaOq] = new StaticRequestHandler(SharedPacketLib.NuxMsgseiLaOq);
        }
    #endregion
}
}
