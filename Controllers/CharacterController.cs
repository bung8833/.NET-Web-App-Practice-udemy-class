using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Services.CharacterService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase // controller w/o View
    {
        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }


        [HttpGet]
        [Route("GetAll")] // More than one Get function
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> Get() 
        {
            return Ok(await _characterService.GetAllCharacters());
        }


        [HttpGet]
        [Route("id")]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> GetSingle(int id)
        {
            return Ok(await _characterService.GetCharacterById(id));
        }


        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> 
            AddCharacter(AddCharacterDto newCharacter)
        {
            return Ok(await _characterService.AddCharacter(newCharacter));
        }


        [HttpPut]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> 
            UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var response = await _characterService.UpdateCharacter(updatedCharacter);

            if (response.Data is null) {
                return NotFound(response);
            }

            return Ok(response);
        }


        [HttpDelete]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> 
            DeleteCharacter(int id)
        {
            var response = await _characterService.DeleteCharacter(id);

            // todo check if character not found
            if (response.Success == false) {
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}