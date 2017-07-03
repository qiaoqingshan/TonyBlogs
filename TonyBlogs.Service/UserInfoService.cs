﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TonyBlogs.Entity;
using TonyBlogs.IService;
using TonyBlogs.DTO.UserInfo;
using TonyBlogs.IRepository;
using AutoMapper;
using TonyBlogs.DTO;
using TonyBlogs.Common;

namespace TonyBlogs.Service
{
    public class UserInfoService : BaseService<UserInfoEntity>,IUserInfoService
    {
        private IUserInfoRepository _userInfoRepository;
        private IUserPurviewService _userPurviewService;

        public UserInfoService(IUserInfoRepository userInfoRepository, 
            IUserPurviewService userPurviewService)
        {
            this._userInfoRepository = userInfoRepository;
            this.baseDal = userInfoRepository;
            this._userPurviewService = userPurviewService;
        }

        public UserInfoSearchDTO GetUserInfoSearchDTO()
        {
            UserInfoSearchDTO dto = new UserInfoSearchDTO();
            dto.PurviewMap = GetPurviewMap();

            return dto;
        }

        public UserInfoListDTO GetUserInfoList(UserInfoSearchDTO searchDTO)
        {
            UserInfoListDTO result = new UserInfoListDTO();

            long totalCount = 0;
            var entityList = this._userInfoRepository.GetUserInfoList(searchDTO, out totalCount);

            result.TotalRecords = totalCount;
            var purviewMap = GetPurviewMap();
            result.List = entityList.Select(m => CreateUserInfoListItemDTO(m, purviewMap)).ToList();

            return result;
        }

        private UserInfoListItemDTO CreateUserInfoListItemDTO(UserInfoEntity entity, Dictionary<long, string> purviewMap)
        {
            var dto = Mapper.DynamicMap<UserInfoListItemDTO>(entity);
            if (purviewMap.ContainsKey(dto.PurviewID))
	        {
                dto.PurviewTitle = purviewMap[dto.PurviewID];
	        }

            return dto;
        }

        public ExecuteResult AddOrEditUserInfo(UserInfoEditDTO dto)
        {
            ExecuteResult result = new ExecuteResult() { IsSuccess = true };

            var entity = Mapper.DynamicMap<UserInfoEntity>(dto);
            

            bool isAdd = dto.UserID == 0;
            if (isAdd)
            {
                entity.LoginPWD = EncryptHelper.Encrypt(entity.LoginPWD);
                entity.UserStatus = Enum.User.UserStatusEnum.Valid;
                entity.InsertTime = DateTime.Now;
                baseDal.Add(entity);
            }
            else
            {
                entity.UpdateTime = DateTime.Now;

                baseDal.UpdateOnly(entity,
                    m => new {
                        m.RealName,
                        m.PurviewID,
                        m.UpdateTime},
                    m => m.UserID == dto.UserID);
            }

            return result;
        }
            
        public UserInfoEditDTO GetUserInfoEditDTO(long userID)
        {
            UserInfoEditDTO dto = new UserInfoEditDTO();

            if (userID <= 0)
            {
                dto.PurviewMap = GetPurviewMap();
                return dto;
            }

            var entity = baseDal.Single(m => m.UserID == userID);
            if (entity == null)
            {
                return dto;
            }

            dto = Mapper.DynamicMap<UserInfoEditDTO>(entity);
            dto.PurviewMap = GetPurviewMap();

            return dto;
        }

        public ExecuteResult DeleteUserInfo(long userID)
        {
            ExecuteResult result = new ExecuteResult() { IsSuccess = true };

            var entity = baseDal.Single(m => m.UserID == userID);

            if (entity == null)
            {
                result.IsSuccess = false;
                result.Message = "当前功能实体不存在";
            }

            entity.UserStatus = Enum.User.UserStatusEnum.Deleted;
            entity.UpdateTime = DateTime.Now;

            baseDal.UpdateOnly(entity, m => new { m.UserStatus, m.UpdateTime }, m => m.UserID == userID);

            return result;
        }

        private Dictionary<long, string> GetPurviewMap()
        {
            return _userPurviewService.GetPurviewMap();
        }
    }
}
