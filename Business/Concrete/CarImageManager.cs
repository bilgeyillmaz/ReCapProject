using Business.Abstract;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Helpers.FileHelper;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CarImageManager : ICarImageService
    {
        ICarImageDal _carImageDal;

        public CarImageManager(ICarImageDal carImageDal)
        {
            _carImageDal = carImageDal;
        }

        public IResult Add(IFormFile carImageFile, CarImage carImage)
        {
            IResult result = BusinessRules.Run(CheckIfImageCountIsCorrect(carImage.CarId));
            if(result!= null)
            {
                return result;  
            }
            var imageResult = FileHelper.Upload(carImageFile);
            if(!imageResult.Success)
            {
                return new ErrorResult("");
            }
            carImage.ImagePath = imageResult.Message;
            carImage.Date = DateTime.Now;   
            _carImageDal.Add(carImage);
            return new SuccessResult();
        }

        public IResult Delete(CarImage carImage)
        {
            var result = _carImageDal.Get(c => c.Id == carImage.Id);
            if(result != null)
            {
                return new ErrorResult(Messages.ImageNotFound);   
            }
            FileHelper.Delete(carImage.ImagePath);  
            _carImageDal.Delete(carImage);
            return new SuccessResult(Messages.ImageDeleted);
        }

        public IDataResult<List<CarImage>> GetAll()
        {
            return new SuccessDataResult<List<CarImage>>(_carImageDal.GetAll()); 
        }

        public IDataResult<CarImage> GetById(int carImageId)
        {
            return new SuccessDataResult<CarImage>(_carImageDal.Get(c=> c.Id == carImageId));
        }

        public IResult Update(IFormFile carImageFile, CarImage carImage)
        {
            var updatedFile = FileHelper.Update(carImageFile, carImage.ImagePath);
            if(!updatedFile.Success)
            {
                return new ErrorResult(updatedFile.Message);
            }
            carImage.ImagePath = updatedFile.Message;
            _carImageDal.Update(carImage);
            return new SuccessResult(Messages.ImageUpdated);
        }

        private IResult CheckIfImageCountIsCorrect(int carImageId)
        {
            var result = _carImageDal.GetAll(c => c.CarId == carImageId).Count;
            if (result > 5)
            {
                return new ErrorResult(Messages.CarImageLimit);
            }
            return new SuccessResult();
        }
    }
}
