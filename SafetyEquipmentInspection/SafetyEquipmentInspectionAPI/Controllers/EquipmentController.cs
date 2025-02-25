﻿using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SafetyEquipmentInspectionAPI.Constants;
using SafetyEquipmentInspectionAPI.DTOs;
using SafetyEquipmentInspectionAPI.Interfaces;

namespace SafetyEquipmentInspectionAPI.Controllers
{
    [ApiController]
    public class EquipmentController : IEquipmentController
    {
        public readonly FirestoreDb _db;
        public EquipmentController()
        {
            Environment.SetEnvironmentVariable(FirestoreConstants.GoogleApplicationCredentials, FirestoreConstants.GoogleApplicationCredentialsPath);
            _db = FirestoreDb.Create(FirestoreConstants.ProjectId);
        }

        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        [HttpGet("/equipment/item/{id}")]
        public async Task<string> GetItem(string id)
        {
            try
            {
                //get Equipment collection from NoSQL db
                CollectionReference equipmentCollection = _db.Collection("Equipment");
                //query collection for document with an EquipmentId equal to id and get async snapshot of query result
                DocumentSnapshot equipmentDocument = await equipmentCollection.Document(id).GetSnapshotAsync();
                //if document exists, use FireStore ConvertTo function to convert it to a DTO
                EquipmentDto equipmentItem = equipmentDocument.ConvertTo<EquipmentDto>();
                return equipmentDocument.Exists ? JsonConvert.SerializeObject(equipmentItem, settings) :
                $"Item with ID {id} not found and may not be in the database. Try viewing the equipment list to check if this item is in the database, then try again with a valid ID.";
            }
            catch (Exception ex)
            {
                //if document
                return $"The exception {ex.GetBaseException().Message} is being thrown from {ex.TargetSite} in {ex.Source}. Please refer to {ex.HelpLink} to search for this exception.";

            }
        }

        [HttpGet("equipment/items/")]
        public async Task<List<EquipmentDto>> GetListItems(string equipmentType = "")
        {
            try
            {
                List<EquipmentDto> equipmentItems = new List<EquipmentDto>();
                CollectionReference equipmentCollection = _db.Collection("Equipment");
                QuerySnapshot getAllItemsQuery = !String.IsNullOrEmpty(equipmentType) ?
                    await equipmentCollection.WhereEqualTo("EquipmentType", equipmentType).GetSnapshotAsync() :
                    await equipmentCollection.GetSnapshotAsync();
                if (getAllItemsQuery.Any())
                {
                    foreach (DocumentSnapshot item in getAllItemsQuery.Documents)
                    {
                        EquipmentDto itemDataTransferObj = item.ConvertTo<EquipmentDto>();
                        equipmentItems.Add(itemDataTransferObj);
                    }
                }

                return equipmentItems;
            }
            catch (Exception ex)
            {
                throw new Exception($"The exception {ex.GetBaseException().Message} is being thrown from {ex.TargetSite} in {ex.Source}. Please refer to {ex.HelpLink} to search for this exception.");
            }
        }

        [HttpPost("equipment/addItem")]
        public async Task<string> AddEquipmentPiece(string equipmentType, string building, int floor, string location)
        {
            try
            {
                EquipmentDto equipmentDto = new EquipmentDto
                {
                    EquipmentId = Guid.NewGuid(),
                    EquipmentType = equipmentType.ToLower(),
                    Building = building.ToUpper(),
                    Floor = floor,
                    Location = location.ToLower()
                };
                string message;
                CollectionReference equipmentCollection = _db.Collection("Equipment");
                string equipmentDtoJson = JsonConvert.SerializeObject(equipmentDto);
                Dictionary<string, object> itemDocDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(equipmentDtoJson);
                //check if document already exists with the equipment ID
                DocumentSnapshot itemDoc = await equipmentCollection.Document(equipmentDto.EquipmentId.ToString()).GetSnapshotAsync();

                if (!itemDoc.Exists)
                {
                    WriteResult docAdded = await equipmentCollection.Document(equipmentDto.EquipmentId.ToString()).SetAsync(itemDocDictionary);
                    message = JsonConvert.SerializeObject(new { message = $"Successfully added item {equipmentDto.EquipmentId}", item = equipmentDtoJson }, settings);
                }
                else
                {
                    message = $"Item {equipmentDto.EquipmentId} already in Equipment. Try again with a different ID";
                }
                return message;

            }
            catch (Exception ex)
            {

                return $"The exception {ex.GetBaseException().Message} is being thrown from {ex.TargetSite} in {ex.Source}. Please refer to {ex.HelpLink} to search for this exception.";
            }

        }

        [HttpPut("equipment/updateItem/{equipmentId}")]
        public async Task<string> UpdateItem(string equipmentId, string equipmentType, string location, int floor, string building)
        {
            try
            {
                CollectionReference equipmentCollection = _db.Collection("Equipment");
                DocumentSnapshot itemDocToBeUpdated = await equipmentCollection.Document(equipmentId).GetSnapshotAsync();

                if (itemDocToBeUpdated.Exists)
                {
                    EquipmentDto equipmentDto = itemDocToBeUpdated.ConvertTo<EquipmentDto>();
                    equipmentDto.EquipmentId = Guid.Parse(equipmentId);
                    equipmentDto.EquipmentType = equipmentType;
                    equipmentDto.Location = location;
                    equipmentDto.Building = building;
                    equipmentDto.Floor = floor;
                    //get async snapshot of this document; null if query.document.Count = 0 (meaning the equipmentId was not found)
                    string dtoJson = JsonConvert.SerializeObject(equipmentDto);
                    Dictionary<string, object> updatesDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(dtoJson);
                    await equipmentCollection.Document(equipmentId).UpdateAsync(updatesDictionary);
                };
                return JsonConvert.SerializeObject(new { message = $"Update of item {equipmentId} successful" });
            }
            catch (Exception ex)
            {

                throw new Exception($"The exception {ex.GetBaseException().Message} is being thrown from {ex.TargetSite} in {ex.Source}. Please refer to {ex.HelpLink} to search for this exception.");
            }

        }

        [HttpDelete("equipment/deleteItem/{id}")]
        public async Task<string> DeleteItem(string id)
        {
            try
            {
                //get Equipment collection from NoSQL db
                CollectionReference equipmentcollection = _db.Collection("Equipment");

                //query collection for document with an EquipmentId equal to id and get async snapshot of query result
                QuerySnapshot query = await equipmentcollection.WhereEqualTo("EquipmentId", id.ToString()).GetSnapshotAsync();
                await equipmentcollection.Document(query.Documents[0].Id).DeleteAsync();
                return $"Deletion of {query.Documents[0].Id} successful.";
            }
            catch (Exception ex)
            {
                return $"The exception {ex.GetBaseException().Message} is being thrown from {ex.TargetSite} in {ex.Source}. Please refer to {ex.HelpLink} to search for this exception.";
            }
        }
    }
}
